using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetDetails;

public sealed class GetUserDetailsHandlerTests
{
    [Fact(DisplayName = "User details handler should return a user from the read repository")]
    public async Task HandleAsync_Should_ReturnUser_When_UserExists()
    {
        var userId = Guid.CreateVersion7();
        var readModel = new UserDetailsReadModel(
            userId,
            "hero@example.com",
            "hero",
            true,
            "Active",
            "Active",
            []);
        var repository = Substitute.For<IUsersReadRepository>();
        repository.GetDetailsByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(readModel);
        var handler = new GetUserDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetUserDetailsQuery(userId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
    }

    [Fact(DisplayName = "User details handler should return not found for a missing user")]
    public async Task HandleAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        var repository = Substitute.For<IUsersReadRepository>();
        repository.GetDetailsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((UserDetailsReadModel?)null);
        var handler = new GetUserDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetUserDetailsQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "User was not found.");
    }
}
