using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersWriteRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "AddAsync should persist a valid user when command is valid")]
    public async Task AddAsync_Should_PersistUser_When_CommandIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            " HERO@Example.COM ",
            " hero ",
            isConfirmed: false,
            [new Claim("permission", "heroes.read")],
            UserStatus.Active.Name,
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);

        user.Email.ShouldBe("hero@example.com");
        user.UserName.ShouldBe("hero");
        user.EmailConfirmed.ShouldBeFalse();
        user.Status.ShouldBe(UserStatus.Active.Name);
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "UpdateAsync should persist user changes when user exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "original@example.com",
            "original-hero",
            isConfirmed: false,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            var result = await repository.UpdateAsync(
                userId,
                "updated@example.com",
                "updated-hero",
                true,
                [new Claim("permission", "heroes.manage")],
                UserStatus.Blocked.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);

        user.Email.ShouldBe("updated@example.com");
        user.UserName.ShouldBe("updated-hero");
        user.EmailConfirmed.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Blocked.Name);
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "DeleteAsync should remove an existing user when user exists")]
    public async Task DeleteAsync_Should_RemoveUser_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "delete@example.com",
            "delete-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.DeleteAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        (await dbContext.Set<ApplicationUser>()
                .AnyAsync(item => item.Id == userId, cancellationToken))
            .ShouldBeFalse();
    }

    [Fact(DisplayName = "Users write repository should return not found for a missing user when user does not exist")]
    public async Task DeleteAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();

        var result = await repository.DeleteAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "BlockAsync should block an active user when user is active")]
    public async Task BlockAsync_Should_BlockUser_When_UserIsActive()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "block@example.com",
            "block-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.BlockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Blocked.Name);
    }

    [Fact(DisplayName = "UnblockAsync should activate a blocked user when user is blocked")]
    public async Task UnblockAsync_Should_ActivateUser_When_UserIsBlocked()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "unblock@example.com",
            "unblock-hero",
            isConfirmed: true,
            [],
            UserStatus.Blocked.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.UnblockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Active.Name);
    }

    private async Task<Guid> AddUserAsync(
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
        var result = await repository.AddAsync(
            email,
            userName,
            Password,
            isConfirmed,
            claims,
            status,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }

    private async Task<string> ReadStatusAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

        return await dbContext.Set<ApplicationUser>()
            .Where(item => item.Id == userId)
            .Select(item => item.Status)
            .SingleAsync(cancellationToken);
    }
}
