using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Matches.Entities.MatchParticipants;

public sealed class MatchParticipantTests
{
    [Fact(DisplayName = "Participant should capture ticket selection when created by match")]
    public void Create_Should_CaptureTicketSelection_When_CreatedByMatch()
    {
        var ticket = Ticket();
        var matchId = MatchId.New();

        var participant = MatchParticipant.Create(matchId, ticket);

        participant.MatchId.ShouldBe(matchId);
        participant.TicketId.ShouldBe(ticket.Id);
        participant.PlayerId.ShouldBe(ticket.PlayerId);
        participant.BuildId.ShouldBe(ticket.BuildId);
        participant.Rating.ShouldBe(ticket.Rating);
    }
}
