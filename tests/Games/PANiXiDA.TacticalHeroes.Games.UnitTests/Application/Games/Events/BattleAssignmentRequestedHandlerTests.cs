using PANiXiDA.TacticalHeroes.Games.Application.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.UnitTests.Games;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Application.Games.Events;

public sealed class BattleAssignmentRequestedHandlerTests
{
    [Theory(DisplayName = "Handler should assign one prepared battle when delivery is repeated")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_AssignPreparedBattle_When_DeliveryIsRepeated(bool alreadyAssigned)
    {
        var ct = TestContext.Current.CancellationToken;
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        if (alreadyAssigned) { game.AssignBattle(battleId); }
        var repository = Substitute.For<IGamesRepository>();
        repository.GetByIdAsync(game.Id, ct).Returns(game);
        var handler = new BattleAssignmentRequestedHandler(repository);
        var request = new BattleAssignmentRequested(game.Id.Value, battleId.Value);

        await handler.HandleAsync(request, ct);

        game.BattleId.ShouldBe(battleId);
        game.Status.ShouldBe(GameStatus.WaitingForPlayers);
        game.StartedAt.ShouldBeNull();
        repository.Received(alreadyAssigned ? 0 : 1).Update(game);
    }

    [Fact(DisplayName = "Handler should reject assignment when game does not exist")]
    public async Task HandleAsync_Should_RejectAssignment_When_GameDoesNotExist()
    {
        var repository = Substitute.For<IGamesRepository>();
        var handler = new BattleAssignmentRequestedHandler(repository);
        var request = new BattleAssignmentRequested(Guid.CreateVersion7(), Guid.CreateVersion7());

        await Should.ThrowAsync<InvalidOperationException>(() => handler.HandleAsync(
            request,
            TestContext.Current.CancellationToken));

        repository.DidNotReceive().Update(Arg.Any<PANiXiDA.TacticalHeroes.Games.Domain.Games.Game>());
    }
}
