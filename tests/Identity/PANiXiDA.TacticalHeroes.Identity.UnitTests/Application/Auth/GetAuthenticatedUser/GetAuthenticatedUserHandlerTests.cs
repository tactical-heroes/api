using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserHandlerTests
{
    [Fact(DisplayName = "Authenticated user handler should return the authentication model when service succeeds")]
    public async Task HandleAsync_Should_ReturnAuthenticationModel_When_ServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var user = new AuthenticatedUserReadModel(userId, "hero@example.test", "hero", "stamp", []);
        var service = Substitute.For<IAuthenticatedUserService>();
        service.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        var sessions = Substitute.For<IUserSessionService>();
        var handler = new GetAuthenticatedUserHandler(service, sessions);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new GetAuthenticatedUserQuery(userId, "stamp"), cancellationToken);

        result.Value.ShouldBe(user);
        await service.Received(1).GetByIdAsync(userId, cancellationToken);
    }

    [Theory(DisplayName = "Authenticated user handler should reject stale proof when security stamp differs")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("new-stamp")]
    public async Task HandleAsync_Should_RejectStaleProof_When_SecurityStampDiffers(string? stamp)
    {
        var userId = Guid.CreateVersion7();
        var user = new AuthenticatedUserReadModel(userId, "hero@example.test", "hero", stamp, []);
        var service = Substitute.For<IAuthenticatedUserService>();
        service.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        var handler = new GetAuthenticatedUserHandler(service, Substitute.For<IUserSessionService>());

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(userId, "old-stamp"), TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Theory(
        DisplayName = "User handler should check authorization after claims when authorization is supplied")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_CheckAuthorizationAfterClaims_When_AuthorizationIsSupplied(bool valid)
    {
        var userId = Guid.CreateVersion7();
        var authorizationId = Guid.CreateVersion7().ToString();
        var user = new AuthenticatedUserReadModel(userId, "hero@example.test", "hero", "new-stamp", []);
        var service = Substitute.For<IAuthenticatedUserService>();
        var sessions = Substitute.For<IUserSessionService>();
        var loaded = false;
        service.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            loaded = true;
            return Result.Success(user);
        });
        sessions.IsAuthorizationValidAsync(userId, authorizationId, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            loaded.ShouldBeTrue();
            return valid;
        });
        var handler = new GetAuthenticatedUserHandler(service, sessions);

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(userId, "old-stamp", authorizationId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBe(valid);
        await sessions.Received(1).IsAuthorizationValidAsync(
            userId, authorizationId, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "User handler should reject invalid proof when identity or stamp is invalid")]
    [InlineData(true, "stamp", "stamp")]
    [InlineData(false, "", "stamp")]
    [InlineData(false, "stamp", null)]
    public async Task HandleAsync_Should_RejectInvalidProof_When_IdentityOrStampIsInvalid(
        bool otherUser,
        string proofStamp,
        string? modelStamp)
    {
        var userId = Guid.CreateVersion7();
        var user = new AuthenticatedUserReadModel(
            otherUser ? Guid.CreateVersion7() : userId, "hero@example.test", "hero", modelStamp, []);
        var service = Substitute.For<IAuthenticatedUserService>();
        service.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        var sessions = Substitute.For<IUserSessionService>();
        var handler = new GetAuthenticatedUserHandler(service, sessions);

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(userId, proofStamp, Guid.CreateVersion7().ToString()),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
        await sessions.DidNotReceiveWithAnyArgs().IsAuthorizationValidAsync(default, "", default);
    }
}
