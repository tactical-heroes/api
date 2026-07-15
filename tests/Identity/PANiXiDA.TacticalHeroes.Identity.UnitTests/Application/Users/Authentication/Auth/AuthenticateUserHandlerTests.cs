using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Auth;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Authentication.Auth;

public sealed class AuthenticateUserHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private const string Email = "hero@example.com";
    private const string Password = "StrongPassword1";

    [Fact(DisplayName = "Handle should return unauthorized when user does not exist")]
    public async Task Handle_Should_ReturnUnauthorized_When_UserDoesNotExist()
    {
        var userCredentialsService = Substitute.For<IUserCredentialsService>();
        userCredentialsService
            .AuthenticateAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("Invalid credentials.")));
        var handler = CreateHandler(userCredentialsService);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Handle should return unauthorized when password is invalid")]
    public async Task Handle_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var userCredentialsService = Substitute.For<IUserCredentialsService>();
        userCredentialsService
            .AuthenticateAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("Invalid credentials.")));
        var handler = CreateHandler(userCredentialsService);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Handle should return forbidden when user is not confirmed")]
    public async Task Handle_Should_ReturnForbidden_When_UserIsNotConfirmed()
    {
        var userCredentialsService = Substitute.For<IUserCredentialsService>();
        userCredentialsService
            .AuthenticateAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Forbidden("Account is not confirmed.")));
        var handler = CreateHandler(userCredentialsService);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Handle should return unauthorized when authenticated read model does not exist")]
    public async Task Handle_Should_ReturnUnauthorized_When_AuthenticatedReadModelDoesNotExist()
    {
        var userCredentialsService = Substitute.For<IUserCredentialsService>();
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        userCredentialsService
            .AuthenticateAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(UserId));
        usersReadRepository
            .GetAuthenticatedUserByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns((AuthenticatedUserReadModel?)null);
        var handler = CreateHandler(
            userCredentialsService,
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
        var authenticatedUser = new AuthenticatedUserReadModel(
            UserId,
            Email,
            ConfirmationStatus: true,
            Roles: ["admin"],
            Claims: [new AuthenticatedUserClaimReadModel("permission", "identity.users.manage")]);
        var userCredentialsService = Substitute.For<IUserCredentialsService>();
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        userCredentialsService
            .AuthenticateAsync(Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(UserId));
        usersReadRepository
            .GetAuthenticatedUserByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(authenticatedUser);
        var handler = CreateHandler(
            userCredentialsService,
            usersReadRepository);

        var result = await handler.HandleAsync(
            new AuthenticateUserCommand(Email, Password),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(authenticatedUser);
    }

    private static AuthenticateUserHandler CreateHandler(
        IUserCredentialsService? userCredentialsService = null,
        IUsersReadRepository? usersReadRepository = null)
    {
        return new AuthenticateUserHandler(
            userCredentialsService ?? Substitute.For<IUserCredentialsService>(),
            usersReadRepository ?? Substitute.For<IUsersReadRepository>());
    }
}
