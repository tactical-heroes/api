using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Matches.Entities.MatchParticipants;

public sealed class MatchParticipantTests
{
    [Fact(DisplayName = "Participant should capture search selection when created by match")]
    public void Create_Should_CaptureSearchSelection_When_CreatedByMatch()
    {
        var search = CreateSearch();
        var matchId = MatchId.New();

        var participant = MatchParticipant.Create(matchId, search);

        participant.MatchId.ShouldBe(matchId);
        participant.MatchSearchId.ShouldBe(search.Id);
        participant.PlayerId.ShouldBe(search.PlayerId);
        participant.BuildId.ShouldBe(search.BuildId);
        participant.Rating.ShouldBe(search.Rating);
    }
}
