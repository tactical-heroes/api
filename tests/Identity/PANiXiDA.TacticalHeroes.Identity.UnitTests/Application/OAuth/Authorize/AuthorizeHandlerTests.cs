using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Authorize;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Authorize;

public sealed class AuthorizeHandlerTests
{
    [Theory(DisplayName = "HandleAsync should return availability when repository responds")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_Should_ReturnAvailability_When_RepositoryResponds(bool isAvailable)
    {
        var userId = Guid.CreateVersion7();
        var expected = isAvailable
            ? Result.Success()
            : Result.Failure(error: Error.Forbidden(message: "User is blocked."));
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.CheckAvailabilityByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(expected);
        var handler = new AuthorizeHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new AuthorizeQuery(userId), cancellationToken);

        result.IsSuccess.ShouldBe(expected.IsSuccess);
        result.Errors.ShouldBe(expected.Errors);
        if (result.IsSuccess)
        {
            result.Value.UserId.ShouldBe(userId);
        }

        await repository.Received(1).CheckAvailabilityByUserIdAsync(userId, cancellationToken);
    }
}
