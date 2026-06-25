using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class UserAutoIncludeTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Get by id should auto include identity user owned entities")]
    public async Task GetByIdAsync_Should_AutoIncludeUserOwnedEntities()
    {
        var role = Role.Create("admin").Value;
        var user = CreateUser();
        var assignRoleResult = user.AssignRole(role.Id);
        var grantClaimResult = user.GrantClaim("permission", "identity.users.manage");

        assignRoleResult.IsSuccess.ShouldBeTrue();
        grantClaimResult.IsSuccess.ShouldBeTrue();

        user.ClearDomainEvents();

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

            dbContext.Roles.Add(role);
            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var identityUsersRepository =
                scope.ServiceProvider.GetRequiredService<IUsersRepository>();

            var loadedUser = await identityUsersRepository.GetByIdAsync(
                user.Id,
                TestContext.Current.CancellationToken);

            loadedUser.ShouldNotBeNull();
            loadedUser.Roles.Single().RoleId.ShouldBe(role.Id);
            loadedUser.Claims.Single().Type.Value.ShouldBe("permission");
            loadedUser.Claims.Single().Value.Value.ShouldBe("identity.users.manage");
        }
    }

    private static User CreateUser()
    {
        return User.Register(
                Email.Create("auto-include@example.com").Value,
                PasswordHash.Create("password-hash").Value,
                TokenHash.Create("confirmation-token-hash").Value,
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;
    }
}
