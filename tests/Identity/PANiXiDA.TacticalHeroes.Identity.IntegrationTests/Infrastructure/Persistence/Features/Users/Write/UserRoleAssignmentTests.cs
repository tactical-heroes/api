using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Write;

public sealed class UserRoleAssignmentTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "User updates should synchronize roles and sessions when role assignments vary")]
    [InlineData("omit")]
    [InlineData("same")]
    [InlineData("replace")]
    [InlineData("add")]
    [InlineData("clear")]
    public async Task UpdateAsync_Should_SynchronizeRolesAndSessions_When_RoleAssignmentsVary(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        await scope.ServiceProvider.GetRequiredService<IdentityRoleSeeder>().SeedAsync(cancellationToken);
        var roles = await dbContext.Set<ApplicationRole>().ToDictionaryAsync(
            role => role.Name!, role => role.Id, cancellationToken);
        var userId = Guid.CreateVersion7();
        var created = await repository.AddAsync(
            CreateUser(userId, [roles[RoleNames.Developer]]), "StrongPassword1!", cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        var originalStamp = (await dbContext.Set<ApplicationUser>()
            .SingleAsync(user => user.Id == userId, cancellationToken)).SecurityStamp;
        var targetRoles = mode switch
        {
            "same" => new[] { roles[RoleNames.Developer] },
            "replace" => [roles[RoleNames.Moderator]],
            "add" => [roles[RoleNames.Developer], roles[RoleNames.Player]],
            _ => []
        };
        var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var otherId = Guid.CreateVersion7();
        foreach (var subject in new[] { userId, otherId })
        {
            await tokenManager.CreateAsync(
                new OpenIddictTokenDescriptor
                {
                    Subject = subject.ToString(),
                    Status = OpenIddictConstants.Statuses.Valid,
                    Type = OpenIddictConstants.TokenTypeIdentifiers.AccessToken
                },
                cancellationToken);
        }

        var result = await repository.UpdateAsync(
            user: CreateUser(userId, targetRoles),
            cancellationToken: cancellationToken,
            replaceRoles: mode != "omit");

        result.IsSuccess.ShouldBeTrue();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Roles)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);
        var expectedRoles = mode == "omit" ? [roles[RoleNames.Developer]] : targetRoles;
        user.Roles.Select(role => role.RoleId).ShouldBe(expectedRoles, ignoreOrder: true);
        var changed = mode is "replace" or "add" or "clear";
        (user.SecurityStamp != originalStamp).ShouldBe(changed);
        var tokens = await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        tokens.Single(token => token.Subject == userId.ToString()).Status.ShouldBe(changed
            ? OpenIddictConstants.Statuses.Revoked
            : OpenIddictConstants.Statuses.Valid);
        tokens.Single(token => token.Subject == otherId.ToString()).Status.ShouldBe(OpenIddictConstants.Statuses.Valid);
    }

    [Theory(DisplayName = "User persistence should reject unknown roles before writes when role does not exist")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveAsync_Should_RejectRolesBeforeWrites_When_RoleDoesNotExist(bool update)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        if (update)
        {
            var created = await repository.AddAsync(CreateUser(id, []), "StrongPassword1!", cancellationToken);
            created.IsSuccess.ShouldBeTrue();
        }
        var invalidUser = CreateUser(id, [Guid.CreateVersion7()]);

        var succeeded = update
            ? (await repository.UpdateAsync(invalidUser, cancellationToken)).IsSuccess
            : (await repository.AddAsync(invalidUser, "StrongPassword1!", cancellationToken)).IsSuccess;

        succeeded.ShouldBeFalse();
        (await dbContext.Set<ApplicationUser>().CountAsync(cancellationToken)).ShouldBe(update ? 1 : 0);
        (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
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
}
