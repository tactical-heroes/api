using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Application.Players.GetInternalPlayer;

public sealed class GetInternalPlayerHandlerTests
{
    [Fact(DisplayName = "Handler should return stored player when user has a player")]
    public async Task HandleAsync_Should_ReturnStoredPlayer_When_UserHasAPlayer()
    {
        var userId = Guid.CreateVersion7();
        var player = new PlayerReadModel(Guid.CreateVersion7(), 1742);
        var repository = Substitute.For<IPlayersReadRepository>();
        var ct = TestContext.Current.CancellationToken;
        repository.GetByUserIdAsync(userId, ct).Returns(player);
        var handler = new GetInternalPlayerHandler(repository);

        var result = await handler.HandleAsync(new GetInternalPlayerQuery(userId), ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(player);
        await repository.Received(1).GetByUserIdAsync(userId, ct);
    }

    [Fact(DisplayName = "Handler should return not found when player is missing")]
    public async Task HandleAsync_Should_ReturnNotFound_When_PlayerIsMissing()
    {
        var repository = Substitute.For<IPlayersReadRepository>();
        var handler = new GetInternalPlayerHandler(repository);

        var result = await handler.HandleAsync(
            new GetInternalPlayerQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }
}
