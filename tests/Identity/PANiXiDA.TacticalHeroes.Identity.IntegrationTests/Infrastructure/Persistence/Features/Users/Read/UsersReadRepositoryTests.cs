using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

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
            .SingleAsync(readRole => readRole.Id == role.Id.Value, cancellationToken);

        userReadModel.Claims.ShouldHaveSingleItem().Value.ShouldBe("identity.profile.read");
        userReadModel.Roles.ShouldHaveSingleItem().Role!.Name.ShouldBe("admin");
        roleReadModel.Claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.manage");
        roleReadModel.Users.ShouldHaveSingleItem().User!.Email.ShouldBe("hero@example.com");
    }

    [Fact(DisplayName = "Read user should include confirmation and password reset tokens")]
    public async Task ReadUser_Should_IncludeConfirmationAndPasswordResetTokens()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var unconfirmedUser = User.Register(
                "unconfirmed@example.com",
                "password-hash",
                "confirmation-token-hash",
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;
        var confirmedUser = CreateConfirmedUser("confirmed@example.com");
        confirmedUser.RequestPasswordReset(
                "password-reset-token-hash",
                DateTimeOffset.UtcNow.AddHours(1),
                "password-reset-token")
            .IsSuccess.ShouldBeTrue();

        await AddAsync(unconfirmedUser, confirmedUser, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityReadDbContext>();

        var unconfirmedReadModel = await dbContext.Set<UserReadDbModel>()
            .Include(user => user.ConfirmationToken)
            .SingleAsync(user => user.Id == unconfirmedUser.Id.Value, cancellationToken);
        var confirmedReadModel = await dbContext.Set<UserReadDbModel>()
            .Include(user => user.PasswordResetToken)
            .SingleAsync(user => user.Id == confirmedUser.Id.Value, cancellationToken);

        unconfirmedReadModel.ConfirmationToken.ShouldNotBeNull();
        unconfirmedReadModel.ConfirmationToken.TokenHash.ShouldBe("confirmation-token-hash");
        confirmedReadModel.PasswordResetToken.ShouldNotBeNull();
        confirmedReadModel.PasswordResetToken.TokenHash.ShouldBe("password-reset-token-hash");
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
                await usersRepository.AddAsync(firstUser, ct);
                await usersRepository.AddAsync(secondUser, ct);
            },
            cancellationToken);
    }

    private static User CreateConfirmedUser(string email = "hero@example.com")
    {
        const string confirmationTokenHash = "confirmation-token-hash";

        var user = User.Register(
                email,
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
