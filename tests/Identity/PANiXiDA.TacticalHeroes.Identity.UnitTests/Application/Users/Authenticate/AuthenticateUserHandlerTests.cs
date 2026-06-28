using PANiXiDA.Core.SpecificationPattern.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authenticate;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Authenticate;

public sealed class AuthenticateUserHandlerTests
{
    private const string Email = "hero@example.com";
    private const string Password = "StrongPassword1";
    private const string PasswordHash = "password-hash";
    private const string ConfirmationTokenHash = "confirmation-token-hash";

    [Fact(DisplayName = "Handle should return unauthorized when user does not exist")]
    public async Task Handle_Should_ReturnUnauthorized_When_UserDoesNotExist()
    {
        var usersRepository = Substitute.For<IUsersRepository>();
        var handler = CreateHandler(usersRepository);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Handle should return unauthorized when password is invalid")]
    public async Task Handle_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var user = CreateConfirmedUser();
        var usersRepository = Substitute.For<IUsersRepository>();
        var passwordHashingService = Substitute.For<IPasswordHashingService>();
        usersRepository
            .GetBySpecificationAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHashingService
            .VerifyPassword(user.PasswordHash, Password)
            .Returns(false);
        var handler = CreateHandler(
            usersRepository,
            passwordHashingService);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Handle should return forbidden when user is not confirmed")]
    public async Task Handle_Should_ReturnForbidden_When_UserIsNotConfirmed()
    {
        var user = CreateUser();
        var usersRepository = Substitute.For<IUsersRepository>();
        var passwordHashingService = Substitute.For<IPasswordHashingService>();
        usersRepository
            .GetBySpecificationAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHashingService
            .VerifyPassword(user.PasswordHash, Password)
            .Returns(true);
        var handler = CreateHandler(
            usersRepository,
            passwordHashingService);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Handle should return unauthorized when authenticated read model does not exist")]
    public async Task Handle_Should_ReturnUnauthorized_When_AuthenticatedReadModelDoesNotExist()
    {
        var user = CreateConfirmedUser();
        var usersRepository = Substitute.For<IUsersRepository>();
        var passwordHashingService = Substitute.For<IPasswordHashingService>();
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersRepository
            .GetBySpecificationAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHashingService
            .VerifyPassword(user.PasswordHash, Password)
            .Returns(true);
        usersReadRepository
            .GetAuthenticatedUserByIdAsync(user.Id.Value, Arg.Any<CancellationToken>())
            .Returns((AuthenticatedUserReadModel?)null);
        var handler = CreateHandler(
            usersRepository,
            passwordHashingService,
            usersReadRepository);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Handle should return authenticated user when credentials are valid")]
    public async Task Handle_Should_ReturnAuthenticatedUser_When_CredentialsAreValid()
    {
        var user = CreateConfirmedUser();
        var authenticatedUser = new AuthenticatedUserReadModel(
            user.Id.Value,
            Email,
            ConfirmationStatus: true,
            Roles: ["admin"],
            Claims: [new AuthenticatedUserClaimReadModel("permission", "identity.users.manage")]);
        var usersRepository = Substitute.For<IUsersRepository>();
        var passwordHashingService = Substitute.For<IPasswordHashingService>();
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersRepository
            .GetBySpecificationAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHashingService
            .VerifyPassword(user.PasswordHash, Password)
            .Returns(true);
        usersReadRepository
            .GetAuthenticatedUserByIdAsync(user.Id.Value, Arg.Any<CancellationToken>())
            .Returns(authenticatedUser);
        var handler = CreateHandler(
            usersRepository,
            passwordHashingService,
            usersReadRepository);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(authenticatedUser);
    }

    private static AuthenticateUserHandler CreateHandler(
        IUsersRepository? usersRepository = null,
        IPasswordHashingService? passwordHashingService = null,
        IUsersReadRepository? usersReadRepository = null)
    {
        return new AuthenticateUserHandler(
            usersRepository ?? Substitute.For<IUsersRepository>(),
            passwordHashingService ?? Substitute.For<IPasswordHashingService>(),
            usersReadRepository ?? Substitute.For<IUsersReadRepository>());
    }

    private static User CreateConfirmedUser()
    {
        var user = CreateUser();

        user.ConfirmRegistration(
                ConfirmationTokenHash,
                DateTimeOffset.UtcNow)
            .IsSuccess.ShouldBeTrue();

        return user;
    }

    private static User CreateUser()
    {
        return User.Register(
                Email,
                PasswordHash,
                ConfirmationTokenHash,
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;
    }
}
