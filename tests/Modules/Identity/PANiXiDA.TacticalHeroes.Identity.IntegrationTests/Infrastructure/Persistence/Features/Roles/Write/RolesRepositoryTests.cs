using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

using UserTestData = PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users.IntegrationTestData;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid role when command is valid")]
    public async Task AddAsync_Should_PersistRole_When_CommandIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            " Developer ",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("Developer");
        role.NormalizedName.ShouldBe("DEVELOPER");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "UpdateAsync should persist role changes when role exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            "Developer",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
            (await repository.UpdateAsync(
                role: IntegrationTestData.CreateRole(
                    id: roleId,
                    name: "Moderator",
                    claims: ((Claim[])[new Claim("permission", "heroes.manage")]).Select(
                        claim => (claim.Type, claim.Value))),
                currentAccessToken: null,
                cancellationToken: cancellationToken))
                .IsSuccess
                .ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("Moderator");
        role.NormalizedName.ShouldBe("MODERATOR");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "DeleteAsync should remove an existing role when role exists")]
    public async Task DeleteAsync_Should_RemoveRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("Player", [], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
            var result = await repository.DeleteAsync(
                id: roleId,
                currentAccessToken: null,
                cancellationToken: cancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        (await dbContext.Set<ApplicationRole>()
            .AnyAsync(item => item.Id == roleId, cancellationToken))
            .ShouldBeFalse();
    }

    [Theory(DisplayName = "Roles repository should return conflict when role name differs only by casing")]
    [InlineData("Developer")]
    [InlineData("developer")]
    [InlineData("DEVELOPER")]
    public async Task AddAsync_Should_ReturnConflict_When_RoleNameDiffersOnlyByCasing(string name)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;

        (await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: "Developer",
                claims: []),
            cancellationToken))
            .IsSuccess
            .ShouldBeTrue();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: name,
                claims: []),
            cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
    }

    [Theory(DisplayName = "UpdateAsync should return conflict when another role name differs only by casing")]
    [InlineData("Developer")]
    [InlineData("developer")]
    [InlineData("DEVELOPER")]
    public async Task UpdateAsync_Should_ReturnConflict_When_AnotherRoleNameDiffersOnlyByCasing(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddRoleAsync("Developer", [], cancellationToken);
        var roleId = await AddRoleAsync("Moderator", [], cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = await repository.UpdateAsync(
            role: IntegrationTestData.CreateRole(roleId, name, []),
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        await using var verificationScope = Fixture.CreateScope();
        var dbContext = verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);
        role.Name.ShouldBe("Moderator");
        role.NormalizedName.ShouldBe("MODERATOR");
    }

    [Fact(DisplayName = "AddAsync should assign the default when previous flag is cleared")]
    public async Task AddAsync_Should_AssignDefault_When_PreviousFlagIsCleared()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var previous = await dbContext.Set<ApplicationRole>().SingleAsync(role => role.IsDefault, cancellationToken);
        var targetId = Guid.CreateVersion7();
        var role = IntegrationTestData.CreateRole(targetId, "CustomDefault", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var cleared = await repository.UpdateAsync(
            role: IntegrationTestData.CreateRole(previous.Id, previous.Name!, [], isDefault: false),
            currentAccessToken: null,
            cancellationToken: cancellationToken);
        cleared.IsSuccess.ShouldBeTrue();

        var result = await repository.AddAsync(role, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var defaults = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .ToArrayAsync(cancellationToken);
        defaults.ShouldHaveSingleItem().Id.ShouldBe(targetId);
        previous.IsDefault.ShouldBeFalse();
        (await repository.DeleteAsync(
            id: previous.Id,
            currentAccessToken: null,
            cancellationToken: cancellationToken))
            .IsSuccess
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "UpdateAsync should assign the default when previous flag is cleared")]
    public async Task UpdateAsync_Should_AssignDefault_When_PreviousFlagIsCleared()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var previous = await dbContext.Set<ApplicationRole>().SingleAsync(role => role.IsDefault, cancellationToken);
        var targetId = await dbContext.Set<ApplicationRole>()
            .Where(role => role.Name == "Moderator")
            .Select(role => role.Id)
            .SingleAsync(cancellationToken);
        var role = IntegrationTestData.CreateRole(targetId, "CustomDefault", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var cleared = await repository.UpdateAsync(
            role: IntegrationTestData.CreateRole(previous.Id, previous.Name!, [], isDefault: false),
            currentAccessToken: null,
            cancellationToken: cancellationToken);
        cleared.IsSuccess.ShouldBeTrue();

        var result = await repository.UpdateAsync(
            role: role,
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var defaults = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .ToArrayAsync(cancellationToken);
        defaults.ShouldHaveSingleItem().Id.ShouldBe(targetId);
        previous.IsDefault.ShouldBeFalse();
        (await repository.DeleteAsync(
            id: previous.Id,
            currentAccessToken: null,
            cancellationToken: cancellationToken))
            .IsSuccess
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "AddAsync should retain the default when duplicate name rejects change")]
    public async Task AddAsync_Should_RetainDefault_When_DuplicateNameRejectsChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var targetId = Guid.CreateVersion7();
        var role = IntegrationTestData.CreateRole(targetId, "Developer", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = await repository.AddAsync(role, cancellationToken);

        result.IsFailure.ShouldBeTrue();
        var defaultName = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .Select(item => item.Name)
            .SingleAsync(cancellationToken);
        defaultName.ShouldBe("Player");
    }

    [Fact(DisplayName = "UpdateAsync should retain the default when duplicate name rejects change")]
    public async Task UpdateAsync_Should_RetainDefault_When_DuplicateNameRejectsChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var targetId = await dbContext.Set<ApplicationRole>()
            .Where(role => role.Name == "Moderator")
            .Select(role => role.Id)
            .SingleAsync(cancellationToken);
        var role = IntegrationTestData.CreateRole(targetId, "Developer", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = await repository.UpdateAsync(
            role: role,
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.IsFailure.ShouldBeTrue();
        var defaultName = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .Select(item => item.Name)
            .SingleAsync(cancellationToken);
        defaultName.ShouldBe("Player");
    }

    [Fact(DisplayName = "DeleteAsync should return conflict when role is default")]
    public async Task DeleteAsync_Should_ReturnConflict_When_RoleIsDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>().SingleAsync(item => item.IsDefault, cancellationToken);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = await repository.DeleteAsync(
            id: role.Id,
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        var existing = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == role.Id, cancellationToken);
        existing.IsDefault.ShouldBeTrue();
        existing.Name.ShouldBe("Player");
    }

    [Theory(DisplayName = "Role writes should restore sessions when transaction is rolled back")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RoleWrites_Should_RestoreSessions_When_TransactionIsRolledBack(bool delete)
    {
        var ct = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("SessionRole", [new("permission", "identity.roles.read")], ct);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var userId = Guid.CreateVersion7();
        var created = await users.AddAsync(
            UserTestData.CreateUser(userId, "member@example.test", true, [roleId], [], "member", UserStatus.Active),
            "StrongPassword1!",
            ct);
        created.IsSuccess.ShouldBeTrue();
        var before = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == userId, ct);
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var authorization = await authorizations.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = userId.ToString(),
                Status = OpenIddictConstants.Statuses.Valid,
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc
            },
            ct);
        await tokens.CreateAsync(
            new OpenIddictTokenDescriptor
            {
                Subject = userId.ToString(),
                AuthorizationId = await authorizations.GetIdAsync(authorization, ct),
                Type = OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                Status = OpenIddictConstants.Statuses.Valid
            },
            ct);
        var roles = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var result = delete
            ? await roles.DeleteAsync(id: roleId, currentAccessToken: null, cancellationToken: ct)
            : await roles.UpdateAsync(
                role: IntegrationTestData.CreateRole(roleId, "RenamedRole", []),
                currentAccessToken: null,
                cancellationToken: ct);
        var revoked = await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .SingleAsync(ct);
        await transaction.RollbackAsync(ct);

        result.IsSuccess.ShouldBeTrue();
        revoked.Status.ShouldBe(OpenIddictConstants.Statuses.Revoked);
        await using var verification = Fixture.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await persisted.Users.AsNoTracking().SingleAsync(user => user.Id == userId, ct);
        user.SecurityStamp.ShouldBe(before.SecurityStamp);
        var role = await persisted.Set<ApplicationRole>().Include(item => item.Claims).SingleAsync(ct);
        role.Name.ShouldBe("SessionRole");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("identity.roles.read");
        (await persisted.Set<ApplicationUserRole>().SingleAsync(ct)).RoleId.ShouldBe(roleId);
        (await persisted.Set<OpenIddictEntityFrameworkCoreToken<Guid>>().SingleAsync(ct))
            .Status
            .ShouldBe(OpenIddictConstants.Statuses.Valid);
        (await persisted.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>().SingleAsync(ct))
            .Status
            .ShouldBe(OpenIddictConstants.Statuses.Valid);
    }

    [Theory(DisplayName = "Role writes should include new members when concurrent assignment commits")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RoleWrites_Should_IncludeNewMembers_When_ConcurrentAssignmentCommits(bool delete)
    {
        var ct = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("ConcurrentRole", [], ct);
        await using var assignmentScope = Fixture.CreateScope();
        var assignmentDb = assignmentScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        await using var assignmentTransaction = await assignmentDb.Database.BeginTransactionAsync(ct);
        var users = assignmentScope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var userId = Guid.CreateVersion7();
        var created = await users.AddAsync(
            UserTestData.CreateUser(userId, "member@example.test", true, [roleId], [], "member", UserStatus.Active),
            "StrongPassword1!",
            ct);
        created.IsSuccess.ShouldBeTrue();
        var before = await assignmentDb.Users.AsNoTracking().SingleAsync(user => user.Id == userId, ct);
        await using var roleScope = Fixture.CreateScope();
        var roleDb = roleScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        await using var roleTransaction = await roleDb.Database.BeginTransactionAsync(ct);
        var backendId = ((NpgsqlConnection)roleDb.Database.GetDbConnection()).ProcessID;
        var roles = roleScope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var pending = delete
            ? roles.DeleteAsync(id: roleId, currentAccessToken: null, cancellationToken: ct)
            : roles.UpdateAsync(
                role: IntegrationTestData.CreateRole(roleId, "RenamedRole", []),
                currentAccessToken: null,
                cancellationToken: ct);
        var waited = await WaitForLockAsync(pending, backendId, ct);
        await assignmentTransaction.CommitAsync(ct);
        var result = await pending;
        await roleTransaction.CommitAsync(ct);

        waited.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        await using var verification = Fixture.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await persisted.Users.AsNoTracking().SingleAsync(user => user.Id == userId, ct);
        user.SecurityStamp.ShouldNotBe(before.SecurityStamp);
        (await persisted.Set<ApplicationUserRole>().AnyAsync(link => link.RoleId == roleId, ct)).ShouldBe(!delete);
    }

    [Fact(DisplayName = "Role writes should reload committed claims when another role update finishes")]
    public async Task RoleWrites_Should_ReloadCommittedClaims_When_AnotherRoleUpdateFinishes()
    {
        var ct = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("ConcurrentRole", [], ct);
        await using var firstScope = Fixture.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var users = firstScope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var userId = Guid.CreateVersion7();
        var created = await users.AddAsync(
            UserTestData.CreateUser(userId, "member@example.test", true, [roleId], [], "member", UserStatus.Active),
            "StrongPassword1!",
            ct);
        created.IsSuccess.ShouldBeTrue();
        await using var firstTransaction = await firstDb.Database.BeginTransactionAsync(ct);
        var firstRoles = firstScope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var changed = await firstRoles.UpdateAsync(
            role: IntegrationTestData.CreateRole(roleId, "ConcurrentRole", [("permission", "identity.roles.read")]),
            currentAccessToken: null,
            cancellationToken: ct);
        changed.IsSuccess.ShouldBeTrue();
        var intermediate = await firstDb.Users.AsNoTracking().SingleAsync(user => user.Id == userId, ct);
        await using var secondScope = Fixture.CreateScope();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        await using var secondTransaction = await secondDb.Database.BeginTransactionAsync(ct);
        var backendId = ((NpgsqlConnection)secondDb.Database.GetDbConnection()).ProcessID;
        var secondRoles = secondScope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var pending = secondRoles.UpdateAsync(
            role: IntegrationTestData.CreateRole(roleId, "ConcurrentRole", []),
            currentAccessToken: null,
            cancellationToken: ct);
        var waited = await WaitForLockAsync(pending, backendId, ct);
        await firstTransaction.CommitAsync(ct);
        var result = await pending;
        await secondTransaction.CommitAsync(ct);

        waited.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        await using var verification = Fixture.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await persisted.Set<ApplicationRole>().Include(item => item.Claims).SingleAsync(ct);
        role.Claims.ShouldBeEmpty();
        var user = await persisted.Users.SingleAsync(item => item.Id == userId, ct);
        user.SecurityStamp.ShouldNotBe(intermediate.SecurityStamp);
    }

    private async Task<bool> WaitForLockAsync(Task pending, int backendId, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(Fixture.ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT wait_event_type = 'Lock' FROM pg_stat_activity WHERE pid = @pid", connection);
        command.Parameters.AddWithValue("pid", backendId);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!pending.IsCompleted && DateTime.UtcNow < deadline)
        {
            if (await command.ExecuteScalarAsync(ct) is true)
            {
                return true;
            }

            await Task.Delay(10, ct);
        }

        return false;
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: name,
                claims: claims.Select(claim => (claim.Type, claim.Value))),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
