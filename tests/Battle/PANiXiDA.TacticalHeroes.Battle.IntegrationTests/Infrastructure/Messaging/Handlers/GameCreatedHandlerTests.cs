using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Messaging.Handlers;

using SourceEvent = PANiXiDA.TacticalHeroes.Games.Contracts.Games.GameCreated;

namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class GameCreatedHandlerTests
{
    [Fact(DisplayName = "Mapper should preserve event data when game created is handled")]
    public void Handle_Should_PreserveEventData_When_GameCreatedIsHandled()
    {
        var source = new SourceEvent(
            GameId: Guid.CreateVersion7(),
            FirstMatchSearchId: Guid.CreateVersion7(),
            SecondMatchSearchId: Guid.CreateVersion7(),
            FirstPlayerId: Guid.CreateVersion7(),
            FirstBuildId: Guid.CreateVersion7(),
            SecondPlayerId: Guid.CreateVersion7(),
            SecondBuildId: Guid.CreateVersion7());

        var result = GameCreatedHandler.Handle(source);

        result.GameId.ShouldBe(source.GameId);
        result.FirstPlayerId.ShouldBe(source.FirstPlayerId);
        result.FirstBuildId.ShouldBe(source.FirstBuildId);
        result.SecondPlayerId.ShouldBe(source.SecondPlayerId);
        result.SecondBuildId.ShouldBe(source.SecondBuildId);
    }
}
