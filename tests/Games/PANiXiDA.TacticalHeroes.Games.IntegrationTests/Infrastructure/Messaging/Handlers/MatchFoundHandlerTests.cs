using PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

using SourceEvent = PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches.MatchFound;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class MatchFoundHandlerTests
{
    [Fact(DisplayName = "Mapper should preserve event data when MatchFound is handled")]
    public void Handle_Should_PreserveEventData_When_MatchFoundIsHandled()
    {
        var source = new SourceEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1500,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1500,
            "RankedDuel",
            DateTimeOffset.UtcNow);

        var result = MatchFoundHandler.Handle(source);

        result.FirstMatchSearchId.ShouldBe(source.FirstMatchSearchId);
        result.FirstPlayerId.ShouldBe(source.FirstPlayerId);
        result.FirstBuildId.ShouldBe(source.FirstBuildId);
        result.FirstRating.ShouldBe(source.FirstRating);
        result.SecondMatchSearchId.ShouldBe(source.SecondMatchSearchId);
        result.SecondPlayerId.ShouldBe(source.SecondPlayerId);
        result.SecondBuildId.ShouldBe(source.SecondBuildId);
        result.SecondRating.ShouldBe(source.SecondRating);
        result.Mode.ShouldBe(source.Mode);
        result.MatchedAt.ShouldBe(source.MatchedAt);
    }
}
