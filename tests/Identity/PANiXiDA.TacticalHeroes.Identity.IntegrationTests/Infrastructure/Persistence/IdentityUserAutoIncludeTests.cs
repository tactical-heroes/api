using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class IdentityUserAutoIncludeTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Get by id should auto include identity user owned entities")]
    public async Task GetByIdAsync_Should_AutoIncludeIdentityUserOwnedEntities()
    {
        var role = IdentityRole.Create("admin").Value;
        var user = CreateUser();
        var assignRoleResult = user.AssignRole(role.Id);
        var grantPermissionResult = user.GrantPermission("identity.users.manage");

        assignRoleResult.IsSuccess.ShouldBeTrue();
        grantPermissionResult.IsSuccess.ShouldBeTrue();

        user.ClearDomainEvents();

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

            dbContext.IdentityRoles.Add(role);
            dbContext.IdentityUsers.Add(user);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var identityUsersRepository =
                scope.ServiceProvider.GetRequiredService<IIdentityUsersRepository>();

            var loadedUser = await identityUsersRepository.GetByIdAsync(
                user.Id,
                TestContext.Current.CancellationToken);

            loadedUser.ShouldNotBeNull();
            loadedUser.Roles.Single().RoleId.ShouldBe(role.Id);
            loadedUser.DirectPermissions.Single().Name.Value.ShouldBe("identity.users.manage");
        }
    }

    private static IdentityUser CreateUser()
    {
        return IdentityUser.Register(
                Email.Create("auto-include@example.com").Value,
                PasswordHash.Create("password-hash").Value,
                TokenHash.Create("confirmation-token-hash").Value,
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;
    }
}
