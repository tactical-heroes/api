using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetAuthenticatedUserByIdAsync should return confirmed user with roles and claims")]
    public async Task GetAuthenticatedUserByIdAsync_Should_ReturnConfirmedUserWithRolesAndClaims()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var role = Role.Create("admin").Value;
        role.GrantClaim(AuthorizationClaimTypes.Permission, "identity.users.manage").IsSuccess.ShouldBeTrue();

        var user = CreateConfirmedUser();
        user.AssignRole(role.Id.Value).IsSuccess.ShouldBeTrue();
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
        readModel.IsConfirmed.ShouldBeTrue();
        readModel.Roles.ShouldBe(["admin"]);
        readModel.Claims.ShouldBe(
        [
            new AuthorizationClaim(AuthorizationClaimTypes.Permission, "identity.profile.read"),
            new AuthorizationClaim(AuthorizationClaimTypes.Permission, "identity.users.manage")
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
            .SingleAsync(readRole => readRole.Id == role.Id.Value, cancellationToken);

        userReadModel.Claims.ShouldHaveSingleItem().Value.ShouldBe("identity.profile.read");
        userReadModel.Roles.ShouldHaveSingleItem().Role.Name.ShouldBe("admin");
        roleReadModel.Claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.manage");
        roleReadModel.Users.ShouldHaveSingleItem().User.Email.ShouldBe("hero@example.com");
    }

    private async Task AddAsync(
        Role role,
        User user,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var rolesRepository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await rolesRepository.AddAsync(role, ct);
                await usersRepository.AddAsync(user, ct);
            },
            cancellationToken);
    }

    private static User CreateConfirmedUser()
    {
        const string confirmationTokenHash = "confirmation-token-hash";

        var user = User.Register(
                "hero@example.com",
                "password-hash",
                confirmationTokenHash,
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;

        user.ConfirmRegistration(
                confirmationTokenHash,
                DateTimeOffset.UtcNow)
            .IsSuccess.ShouldBeTrue();

        return user;
    }
}
