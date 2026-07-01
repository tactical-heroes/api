using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1";

    [Fact(DisplayName = "GetAuthenticatedUserByIdAsync should return confirmed user with roles and claims")]
    public async Task GetAuthenticatedUserByIdAsync_Should_ReturnConfirmedUserWithRolesAndClaims()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "admin"
        };

        var user = CreateConfirmedUser();
        user.AssignRole(role.Id).IsSuccess.ShouldBeTrue();
        user.GrantClaim(AuthorizationClaimTypes.Permission, "identity.profile.read").IsSuccess.ShouldBeTrue();

        await AddAsync(role, user, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();

        var readModel = await repository.GetAuthenticatedUserByIdAsync(
            user.Id.Value,
            cancellationToken);

        readModel.ShouldNotBeNull();
        readModel.Id.ShouldBe(user.Id.Value);
        readModel.Email.ShouldBe(user.Email.Value);
        readModel.ConfirmationStatus.ShouldBeTrue();
        readModel.Roles.ShouldBe(["admin"]);
        readModel.Claims.ShouldBe(
        [
            new AuthenticatedUserClaimReadModel(AuthorizationClaimTypes.Permission, "identity.profile.read"),
            new AuthenticatedUserClaimReadModel(AuthorizationClaimTypes.Permission, "identity.users.manage")
        ]);

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityReadDbContext>();
        var userReadModel = await dbContext.Set<UserReadDbModel>()
            .Include(readUser => readUser.Claims)
            .Include(readUser => readUser.Roles)
            .ThenInclude(userRole => userRole.Role)
            .SingleAsync(readUser => readUser.Id == user.Id.Value, cancellationToken);
        var roleReadModel = await dbContext.Set<RoleReadDbModel>()
            .Include(readRole => readRole.Claims)
            .Include(readRole => readRole.Users)
            .ThenInclude(userRole => userRole.User)
            .SingleAsync(readRole => readRole.Id == role.Id, cancellationToken);

        userReadModel.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("identity.profile.read");
        userReadModel.Roles.ShouldHaveSingleItem().Role!.Name.ShouldBe("admin");
        roleReadModel.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("identity.users.manage");
        roleReadModel.Users.ShouldHaveSingleItem().User!.Email.ShouldBe("hero@example.com");
    }

    private async Task AddAsync(
        ApplicationRole role,
        User user,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var createRoleResult = await roleManager.CreateAsync(role);

                createRoleResult.Succeeded.ShouldBeTrue();

                var addRoleClaimResult = await roleManager.AddClaimAsync(
                    role,
                    new Claim(AuthorizationClaimTypes.Permission, "identity.users.manage"));

                addRoleClaimResult.Succeeded.ShouldBeTrue();

                await usersRepository.AddAsync(user, Password, ct);
            },
            cancellationToken);
    }

    private async Task AddAsync(
        User firstUser,
        User secondUser,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await usersRepository.AddAsync(firstUser, Password, ct);
                await usersRepository.AddAsync(secondUser, Password, ct);
            },
            cancellationToken);
    }

    private static User CreateConfirmedUser(string email = "hero@example.com")
    {
        var user = User.Register(
                email)
            .Value;

        user.ConfirmRegistration()
            .IsSuccess.ShouldBeTrue();

        return user;
    }
}
