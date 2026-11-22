using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Messaging.Handlers;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class MatchFoundHandlerTests
{
    [Fact(DisplayName = "Mapper should preserve pair and event identity when match was found")]
    public void Handle_Should_PreservePairAndEventIdentity_When_MatchWasFound()
    {
        var source = new MatchFound(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1500,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1510,
            "RankedDuel",
            DateTimeOffset.UtcNow);

        var result = MatchFoundHandler.Handle(source);

        result.Id.ShouldBe(source.Id);
        result.OccurredOnUtc.ShouldBe(source.OccurredOnUtc);
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
