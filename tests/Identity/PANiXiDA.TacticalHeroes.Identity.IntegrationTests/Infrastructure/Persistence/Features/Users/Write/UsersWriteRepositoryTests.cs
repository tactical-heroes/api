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

    [Fact(DisplayName = "Users write repository should persist, update, and delete user state")]
    public async Task Repository_Should_PersistUpdateAndDelete_When_CommandsAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid userId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            var result = await repository.AddAsync(
                " HERO@Example.COM ",
                " hero ",
                Password,
                false,
                [new Claim("permission", "heroes.read")],
                UserStatus.Active.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            userId = result.Value;
        }

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

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var user = await dbContext.Set<ApplicationUser>()
                .Include(item => item.Claims)
                .AsNoTracking()
                .SingleAsync(item => item.Id == userId, cancellationToken);

            user.Email.ShouldBe("updated@example.com");
            user.UserName.ShouldBe("updated-hero");
            user.EmailConfirmed.ShouldBeTrue();
            user.Status.ShouldBe(UserStatus.Blocked.Name);
            var claim = user.Claims.ShouldHaveSingleItem();
            claim.ClaimType.ShouldBe("permission");
            claim.ClaimValue.ShouldBe("heroes.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.DeleteAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            (await dbContext.Set<ApplicationUser>()
                    .AnyAsync(item => item.Id == userId, cancellationToken))
                .ShouldBeFalse();
        }
    }

    [Fact(DisplayName = "Users write repository should block and unblock an existing user")]
    public async Task Repository_Should_UpdateStatus_When_UserIsBlockedAndUnblocked()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid userId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            userId = (await repository.AddAsync(
                "status@example.com",
                "status-hero",
                Password,
                true,
                [],
                UserStatus.Active.Name,
                cancellationToken)).Value;
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.BlockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Blocked.Name);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.UnblockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Active.Name);
    }

    [Fact(DisplayName = "Users write repository should return not found for a missing user")]
    public async Task Repository_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();

        var result = await repository.DeleteAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
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
