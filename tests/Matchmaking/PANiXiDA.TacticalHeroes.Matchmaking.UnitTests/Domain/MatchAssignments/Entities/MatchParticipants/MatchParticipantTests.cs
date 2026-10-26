using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.Entities.MatchParticipants;

public sealed class MatchParticipantTests
{
    [Fact(DisplayName = "Matchmaking should capture ticket selection when created by assignment")]
    public void Create_Should_CaptureTicketSelection_When_CreatedByAssignment()
    {
        var ticket = Ticket();
        var assignmentId = MatchAssignmentId.New();

        var participant = MatchParticipant.Create(assignmentId, ticket);

        participant.MatchAssignmentId.ShouldBe(assignmentId);
        participant.TicketId.ShouldBe(ticket.Id);
        participant.PlayerId.ShouldBe(ticket.PlayerId);
        participant.BuildSnapshotId.ShouldBe(ticket.BuildSnapshotId);
        participant.AcceptedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "Matchmaking should retain first timestamp when repeated")]
    public void Confirm_Should_RetainFirstTimestamp_When_Repeated()
    {
        var participant = MatchParticipant.Create(MatchAssignmentId.New(), Ticket());

        participant.Confirm(At(2));
        participant.Confirm(At(3));

        participant.AcceptedAt.ShouldBe(At(2));
    }
}
