using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authenticate;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Authenticate;

public sealed class GetAuthenticatedUserHandlerTests
{
    [Fact(DisplayName = "Handle should return not found when user does not exist")]
    public async Task Handle_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        var repository = Substitute.For<IUsersReadRepository>();
        repository
            .GetAuthenticatedUserByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AuthenticatedUserReadModel?)null);
        var handler = new GetAuthenticatedUserHandler(repository);

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Handle should return forbidden when confirmation status is false")]
    public async Task Handle_Should_ReturnForbidden_When_ConfirmationStatusIsFalse()
    {
        var user = new AuthenticatedUserReadModel(
            Guid.CreateVersion7(),
            "hero@example.com",
            ConfirmationStatus: false,
            Roles: [],
            Claims: []);
        var repository = Substitute.For<IUsersReadRepository>();
        repository
            .GetAuthenticatedUserByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        var handler = new GetAuthenticatedUserHandler(repository);

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(user.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Handle should return authenticated user when confirmation status is true")]
    public async Task Handle_Should_ReturnAuthenticatedUser_When_ConfirmationStatusIsTrue()
    {
        var user = new AuthenticatedUserReadModel(
            Guid.CreateVersion7(),
            "hero@example.com",
            ConfirmationStatus: true,
            Roles: ["admin"],
            Claims: [new AuthenticatedUserClaimReadModel("permission", "identity.users.manage")]);
        var repository = Substitute.For<IUsersReadRepository>();
        repository
            .GetAuthenticatedUserByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        var handler = new GetAuthenticatedUserHandler(repository);

        var result = await handler.HandleAsync(
            new GetAuthenticatedUserQuery(user.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(user);
    }
}
