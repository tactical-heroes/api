using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    [Fact(
        DisplayName = "Change password handler should delegate password change to credentials service when credentials service succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.ChangePasswordAsync(
            userId,
            "access-token",
            "CurrentPassword1!",
            "NewPassword1!",
            Arg.Any<CancellationToken>())
            .Returns(Result.Success(new AuthenticatedUserReadModel(userId, "hero@example.test", "hero", [])));
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.AccessToken.Returns("access-token");
        var handler = new ChangePasswordHandler(service, currentUser);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ChangePasswordCommand(userId, "CurrentPassword1!", "NewPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ChangePasswordAsync(
            userId,
            "access-token",
            "CurrentPassword1!",
            "NewPassword1!",
            cancellationToken);
    }

    [Theory(DisplayName = "Password change requires an access token from the current user")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_CurrentAccessTokenIsMissing(string? accessToken)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.AccessToken.Returns(accessToken);
        var service = Substitute.For<IUserCredentialsService>();
        var handler = new ChangePasswordHandler(service, currentUser);

        var result = await handler.HandleAsync(
            new ChangePasswordCommand(Guid.CreateVersion7(), "CurrentPassword1!", "NewPassword1!"),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Unauthorized);
        service.ReceivedCalls().ShouldBeEmpty();
    }
}
