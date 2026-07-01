using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class UserAutoIncludeTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1";

    [Fact(DisplayName = "Get by id should restore identity user details")]
    public async Task GetByIdAsync_Should_RestoreUserDetails()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "admin"
        };
        var user = CreateUser();
        var assignRoleResult = user.AssignRole(role.Id);
        var grantClaimResult = user.GrantClaim("permission", "identity.users.manage");

        assignRoleResult.IsSuccess.ShouldBeTrue();
        grantClaimResult.IsSuccess.ShouldBeTrue();

        user.ClearDomainEvents();

        await using (var scope = Fixture.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    var createRoleResult = await roleManager.CreateAsync(role);

                    createRoleResult.Succeeded.ShouldBeTrue();

                    await usersRepository.AddAsync(user, Password, ct);
                },
                TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var identityUsersRepository =
                scope.ServiceProvider.GetRequiredService<IUsersRepository>();

            var loadedUser = await identityUsersRepository.GetByIdAsync(
                user.Id.Value,
                TestContext.Current.CancellationToken);

            loadedUser.ShouldNotBeNull();
            loadedUser.RoleIds.Single().Value.ShouldBe(role.Id);
            loadedUser.Claims.Single().Type.Value.ShouldBe("permission");
            loadedUser.Claims.Single().Value.Value.ShouldBe("identity.users.manage");
        }
    }

    private static User CreateUser()
    {
        return User.Register(
                "auto-include@example.com")
            .Value;
    }
}
