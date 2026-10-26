using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.Entities.MatchParticipants;

public sealed class MatchParticipantTests
{
    [Fact(DisplayName = "Participant should capture ticket selection when created by assignment")]
    public void Create_Should_CaptureTicketSelection_When_CreatedByAssignment()
    {
        var ticket = Ticket();
        var assignmentId = MatchAssignmentId.New();

        var participant = MatchParticipant.Create(assignmentId, ticket);

        participant.MatchAssignmentId.ShouldBe(assignmentId);
        participant.TicketId.ShouldBe(ticket.Id);
        participant.PlayerId.ShouldBe(ticket.PlayerId);
        participant.BuildSnapshotId.ShouldBe(ticket.BuildSnapshotId);
        participant.Rating.ShouldBe(ticket.Rating);
    }
}
