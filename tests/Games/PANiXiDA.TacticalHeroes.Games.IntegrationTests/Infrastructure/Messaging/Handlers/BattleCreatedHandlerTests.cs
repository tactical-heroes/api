using PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

using SourceEvent = PANiXiDA.TacticalHeroes.Battle.Contracts.Battles.BattleCreated;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class BattleCreatedHandlerTests
{
    [Fact(DisplayName = "Mapper should preserve event data when battle created is handled")]
    public void Handle_Should_PreserveEventData_When_BattleCreatedIsHandled()
    {
        var source = new SourceEvent(
            GameId: Guid.CreateVersion7(),
            BattleId: Guid.CreateVersion7());

        var result = BattleCreatedHandler.Handle(source);

        result.GameId.ShouldBe(source.GameId);
        result.BattleId.ShouldBe(source.BattleId);
    }
}
