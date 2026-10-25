using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Logout;

public sealed class LogoutHandlerTests
{
    [Theory(DisplayName = "Logout handler should return session result when session service completes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnSessionResult_When_ServiceCompletes(bool failure)
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new LogoutCommand(Guid.CreateVersion7(), Guid.CreateVersion7().ToString(), "client");
        var sessions = Substitute.For<IUserSessionService>();
        var expected = failure ? Result.Failure(Error.Unauthorized(message: "Invalid session.")) : Result.Success();
        sessions.RevokeSessionAsync(command.UserId, command.AuthorizationId, command.ClientId, ct).Returns(expected);
        var handler = new LogoutHandler(sessions);

        var result = await handler.HandleAsync(command, ct);

        result.ShouldBe(expected);
        await sessions.Received(1).RevokeSessionAsync(command.UserId, command.AuthorizationId, command.ClientId, ct);
    }
}
