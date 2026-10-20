using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepositoryTests(IntegrationTestFixture fixture)
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
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var result = await repository.UpdateAsync(
                IntegrationTestData.CreateUser(
                    id: userId,
                    email: "updated@example.com",
                    isConfirmed: true,
                    roleIds: [],
                    claims: ((Claim[])[new Claim("permission", "heroes.manage")]).Select(
                        claim => (claim.Type, claim.Value)),
                    userName: "updated-hero",
                    status: UserStatus.Blocked),
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
                .SaveChangesAsync(cancellationToken);
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
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.DeleteAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
                .SaveChangesAsync(cancellationToken);
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
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

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
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.BlockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
                .SaveChangesAsync(cancellationToken);
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
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.UnblockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Active.Name);
    }

    [Theory(DisplayName = "User updates should synchronize roles when role assignments vary")]
    [InlineData("same")]
    [InlineData("replace")]
    [InlineData("add")]
    [InlineData("clear")]
    public async Task UpdateAsync_Should_SynchronizeRoles_When_RoleAssignmentsVary(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        await Fixture.SeedRolesAsync(cancellationToken);
        var roles = await dbContext.Set<ApplicationRole>().ToDictionaryAsync(
            role => role.Name!, role => role.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var userId = Guid.CreateVersion7();
        var created = await repository.AddAsync(
            CreateUser(userId, [roles["Developer"]]), "StrongPassword1!", cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var targetRoles = mode switch
        {
            "same" => new[] { roles["Developer"] },
            "replace" => [roles["Moderator"]],
            "add" => [roles["Developer"], roles["Player"]],
            _ => []
        };

        var result = await repository.UpdateAsync(
            user: CreateUser(userId, targetRoles),
            cancellationToken: cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Roles)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);
        user.Roles.Select(role => role.RoleId).ShouldBe(targetRoles, ignoreOrder: true);
    }

    [Fact(DisplayName = "AddAsync should reject unknown roles before writes when role does not exist")]
    public async Task AddAsync_Should_RejectRolesBeforeWrites_When_RoleDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        var invalidUser = CreateUser(id, [Guid.CreateVersion7()]);

        var result = await repository.AddAsync(invalidUser, Password, cancellationToken);

        result.IsSuccess.ShouldBeFalse();
        (await dbContext.Set<ApplicationUser>().CountAsync(cancellationToken)).ShouldBe(0);
        (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "UpdateAsync should reject unknown roles before writes when role does not exist")]
    public async Task UpdateAsync_Should_RejectRolesBeforeWrites_When_RoleDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        var created = await repository.AddAsync(CreateUser(id, []), Password, cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var invalidUser = CreateUser(id, [Guid.CreateVersion7()]);

        var result = await repository.UpdateAsync(invalidUser, cancellationToken);

        result.IsSuccess.ShouldBeFalse();
        (await dbContext.Set<ApplicationUser>().CountAsync(cancellationToken)).ShouldBe(1);
        (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Theory(DisplayName = "User updates should preserve state when identity validation fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_Should_PreserveState_When_IdentityValidationFails(bool duplicateEmail)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var roles = await dbContext.Set<ApplicationRole>().ToDictionaryAsync(
            role => role.Name!, role => role.Id, cancellationToken);
        var id = Guid.CreateVersion7();
        var created = await repository.AddAsync(
            CreateUser(id, [roles["Player"]]), "StrongPassword1!", cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var other = IntegrationTestData.CreateUser(
            id: Guid.CreateVersion7(),
            email: "occupied@example.com",
            isConfirmed: true,
            roleIds: [],
            claims: [],
            userName: "occupied",
            status: UserStatus.Active);
        (await repository.AddAsync(other, "StrongPassword1!", cancellationToken)).IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var original = await dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleAsync(user => user.Id == id, cancellationToken);
        var updated = IntegrationTestData.CreateUser(
            id: id,
            email: duplicateEmail ? "occupied@example.com" : "hero@example.com",
            isConfirmed: false,
            roleIds: [roles["Developer"]],
            claims: [("permission", "identity.users.read")],
            userName: duplicateEmail ? "hero" : "occupied",
            status: UserStatus.Active);

        var result = await repository.UpdateAsync(updated, cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        var persisted = await dbContext.Set<ApplicationUser>()
            .Include(user => user.Roles)
            .Include(user => user.Claims)
            .AsNoTracking()
            .SingleAsync(user => user.Id == id, cancellationToken);
        persisted.Email.ShouldBe(original.Email);
        persisted.UserName.ShouldBe(original.UserName);
        persisted.EmailConfirmed.ShouldBe(original.EmailConfirmed);
        persisted.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(roles["Player"]);
        persisted.Claims.ShouldBeEmpty();
    }

    private static User CreateUser(Guid id, IReadOnlyCollection<Guid> roleIds) =>
        IntegrationTestData.CreateUser(
            id: id,
            email: "hero@example.com",
            isConfirmed: true,
            roleIds: roleIds,
            claims: [],
            userName: "hero",
            status: UserStatus.Active);

    private async Task<Guid> AddUserAsync(
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateUser(
                id: Guid.CreateVersion7(),
                email: email,
                isConfirmed: isConfirmed,
                roleIds: [],
                claims: claims.Select(claim => (claim.Type, claim.Value)),
                userName: userName,
                status: UserStatus.Create(value: status).Value),
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);

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
