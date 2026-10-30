using PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

using SourceEvent = PANiXiDA.TacticalHeroes.Games.Domain.Games.Events.GameCreated;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class GameCreatedHandlerTests
{
    [Fact(DisplayName = "Mapper should preserve event data when game created is handled")]
    public void Handle_Should_PreserveEventData_When_GameCreatedIsHandled()
    {
        var source = new SourceEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var result = GameCreatedHandler.Handle(source);

        result.GameId.ShouldBe(source.GameId);
        result.FirstMatchSearchId.ShouldBe(source.FirstMatchSearchId);
        result.SecondMatchSearchId.ShouldBe(source.SecondMatchSearchId);
        result.Id.ShouldBe(source.Id);
        result.OccurredOnUtc.ShouldBe(source.OccurredOnUtc);
    }
}
