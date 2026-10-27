using System.Text.Json;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Messaging.Handlers;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Messaging.Handlers;

public sealed class MatchFoundHandlerTests
{
    [Fact(DisplayName = "Handler should preserve identity and payload when event is mapped repeatedly")]
    public void Handle_Should_PreserveIdentityAndPayload_When_EventIsMappedRepeatedly()
    {
        var first = CreateSearch();
        var second = CreateSearch();
        first.MatchWith(second, At(1)).IsSuccess.ShouldBeTrue();
        var domainEvent = first.GetDomainEvents().OfType<MatchFound>().ShouldHaveSingleItem();

        var message = MatchFoundHandler.Handle(domainEvent);
        var retried = MatchFoundHandler.Handle(domainEvent);
        var json = JsonSerializer.Serialize(message);
        var restored = JsonSerializer.Deserialize<Contracts.MatchSearches.MatchFound>(json);

        message.Id.ShouldBe(domainEvent.Id);
        message.OccurredOnUtc.ShouldBe(domainEvent.OccurredOnUtc);
        retried.ShouldBe(message);
        restored.ShouldBe(message);
        message.FirstMatchSearchId.ShouldBe(first.Id.Value);
        message.FirstPlayerId.ShouldBe(first.PlayerId.Value);
        message.FirstBuildId.ShouldBe(first.BuildId.Value);
        message.FirstRating.ShouldBe(first.Rating.Value);
        message.SecondMatchSearchId.ShouldBe(second.Id.Value);
        message.SecondPlayerId.ShouldBe(second.PlayerId.Value);
        message.SecondBuildId.ShouldBe(second.BuildId.Value);
        message.SecondRating.ShouldBe(second.Rating.Value);
        message.Mode.ShouldBe(first.Mode.Name);
        message.MatchedAt.ShouldBe(At(1).Value);
    }
}
