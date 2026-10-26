using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments;

public sealed class MatchAssignmentTests
{
    [Fact(DisplayName = "Matchmaking should capture selections without mutating tickets when created")]
    public void Create_Should_CaptureSelectionsWithoutMutatingTickets_When_Created()
    {
        var first = Ticket();
        var second = Ticket();

        var assignment = MatchAssignment.Create(first, second, At(1), At(30), At(60)).Value;

        assignment.Participants.Count.ShouldBe(2);
        assignment.Participants.Select(value => value.Id).Distinct().Count().ShouldBe(2);
        assignment.Participants.ShouldAllBe(value => value.MatchAssignmentId == assignment.Id);
        var participant = assignment.Participants.Single(value => value.TicketId == first.Id);
        participant.PlayerId.ShouldBe(first.PlayerId);
        participant.BuildSnapshotId.ShouldBe(first.BuildSnapshotId);
        participant.Rating.ShouldBe(first.Rating);
        first.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        assignment.Status.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
        Should.Throw<NotSupportedException>(() => ((ICollection<MatchParticipant>)assignment.Participants).Clear());
    }

    [Fact(DisplayName = "Matchmaking should reject incompatible tickets when created")]
    public void Create_Should_RejectIncompatibleTickets_When_Created()
    {
        var first = Ticket();

        MatchAssignment.Create(first, first, At(1), At(30), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(first.PlayerId.Value), At(1), At(30), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(mode: "other"), At(1), At(30), At(60)).IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "Matchmaking should reject invalid deadlines when created")]
    [InlineData(1, 60)]
    [InlineData(0, 60)]
    [InlineData(30, 30)]
    [InlineData(30, 29)]
    public void Create_Should_RejectInvalidDeadlines_When_Created(int acceptance, int creation)
    {
        var result = MatchAssignment.Create(Ticket(), Ticket(), At(1), At(acceptance), At(creation));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should reject unavailable tickets when created")]
    public void Create_Should_RejectUnavailableTickets_When_Created()
    {
        var first = Ticket();
        var second = Ticket();

        MatchAssignment.Create(first, second, At(-1), At(30), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(expiresAt: 1), At(1), At(30), At(60)).IsFailure.ShouldBeTrue();
        second.Reserve(MatchAssignmentId.New(), At(1)).IsSuccess.ShouldBeTrue();
        MatchAssignment.Create(first, second, At(2), At(30), At(60)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should request game creation once when both participants confirm")]
    public void Accept_Should_RequestGameCreationOnce_When_BothParticipantsConfirm()
    {
        var assignment = Assignment();
        var participants = assignment.Participants.ToArray();

        assignment.Accept(participants[0].Id, At(2)).IsSuccess.ShouldBeTrue();
        assignment.Status.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
        assignment.GetDomainEvents().OfType<GameCreationRequested>().ShouldBeEmpty();
        assignment.Accept(participants[0].Id, At(3)).IsSuccess.ShouldBeTrue();
        assignment.Accept(participants[1].Id, At(4)).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Accept(participants[1].Id, At(100)).IsSuccess.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.CreatingGame);
        assignment.GetDomainEvents().OfType<MatchParticipantAccepted>().Count().ShouldBe(2);
        var request = assignment.GetDomainEvents().OfType<GameCreationRequested>().ShouldHaveSingleItem();
        request.MatchAssignmentId.ShouldBe(assignment.Id.Value);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        participants[0].AcceptedAt!.Value.ShouldBe(At(2).Value);
    }

    [Theory(DisplayName = "Matchmaking should reject confirmation when outside window")]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(31)]
    public void Accept_Should_RejectConfirmation_When_OutsideWindow(int now)
    {
        var assignment = Assignment();

        assignment.Accept(assignment.Participants.First().Id, At(now)).IsFailure.ShouldBeTrue();

        assignment.Participants.ShouldAllBe(value => value.AcceptedAt == null);
        assignment.GetDomainEvents().Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Matchmaking should reject outsider when participant is unknown")]
    public void Accept_Should_RejectOutsider_When_ParticipantIsUnknown()
    {
        var assignment = Assignment();

        assignment.Accept(MatchParticipantId.New(), At(2)).IsFailure.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
    }

    [Fact(DisplayName = "Matchmaking should close proposal when participant declines")]
    public void Decline_Should_CloseProposal_When_ParticipantDeclines()
    {
        var assignment = Assignment();

        assignment.Decline(MatchParticipantId.New(), At(2)).IsFailure.ShouldBeTrue();
        assignment.Decline(assignment.Participants.First().Id, At(2)).IsSuccess.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.Cancelled);
        assignment.ClosureReason!.Value.ShouldBe("Participant declined.");
        assignment.Accept(assignment.Participants.Last().Id, At(3)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should preserve proposal when deadline elapsed")]
    public void Decline_Should_PreserveProposal_When_DeadlineElapsed()
    {
        var assignment = Assignment();

        assignment.Decline(assignment.Participants.First().Id, At(30)).IsFailure.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
    }

    [Fact(DisplayName = "Matchmaking should publish once and preserve reason when retried")]
    public void Cancel_Should_PublishOnceAndPreserveReason_When_Retried()
    {
        var assignment = Assignment();
        var reason = MatchmakingReason.Create("Search cancelled").Value;

        assignment.Cancel(reason).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Cancel(MatchmakingReason.Create("Retry").Value).IsSuccess.ShouldBeTrue();

        assignment.ClosureReason.ShouldBe(reason);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        ConfirmedAssignment().Cancel(reason).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should bind exactly one game when delivery is retried")]
    public void Complete_Should_BindExactlyOneGame_When_DeliveryIsRetried()
    {
        var assignment = ConfirmedAssignment();
        var game = GameReference.Create(Guid.CreateVersion7()).Value;

        assignment.Complete(game, At(3)).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Complete(game, At(100)).IsSuccess.ShouldBeTrue();
        assignment.Complete(GameReference.Create(Guid.CreateVersion7()).Value, At(4)).IsFailure.ShouldBeTrue();

        assignment.GameId.ShouldBe(game);
        assignment.Status.ShouldBe(MatchAssignmentStatus.Ready);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        assignment.Expire(At(100)).IsFailure.ShouldBeTrue();
        assignment.Fail(MatchmakingReason.Create("Late failure").Value).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should reject result when not actively creating")]
    public void Complete_Should_RejectResult_When_NotActivelyCreating()
    {
        var game = GameReference.Create(Guid.CreateVersion7()).Value;
        Assignment().Complete(game, At(2)).IsFailure.ShouldBeTrue();
        var assignment = ConfirmedAssignment();

        assignment.Complete(game, At(0)).IsFailure.ShouldBeTrue();
        assignment.Complete(game, At(60)).IsFailure.ShouldBeTrue();
        assignment.Expire(At(60)).IsSuccess.ShouldBeTrue();
        assignment.Complete(game, At(61)).IsFailure.ShouldBeTrue();

        assignment.GameId.ShouldBeNull();
    }

    [Fact(DisplayName = "Matchmaking should close game creation once when retried")]
    public void Fail_Should_CloseGameCreationOnce_When_Retried()
    {
        var assignment = ConfirmedAssignment();
        var reason = MatchmakingReason.Create("Games unavailable").Value;
        Assignment().Fail(reason).IsFailure.ShouldBeTrue();

        assignment.Fail(reason).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Fail(reason).IsSuccess.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.Failed);
        assignment.ClosureReason.ShouldBe(reason);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        assignment.Complete(GameReference.Create(Guid.CreateVersion7()).Value, At(3)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should use active phase deadline and publish once when overdue")]
    public void Expire_Should_UseActivePhaseDeadlineAndPublishOnce_When_Overdue()
    {
        var proposal = Assignment();
        proposal.Expire(At(29)).IsFailure.ShouldBeTrue();
        proposal.Expire(At(30)).IsSuccess.ShouldBeTrue();
        var count = proposal.GetDomainEvents().Count;
        proposal.Expire(At(31)).IsSuccess.ShouldBeTrue();
        proposal.GetDomainEvents().Count.ShouldBe(count);
        proposal.Accept(proposal.Participants.First().Id, At(31)).IsFailure.ShouldBeTrue();

        var creating = ConfirmedAssignment();
        creating.Expire(At(30)).IsFailure.ShouldBeTrue();
        creating.Expire(At(60)).IsSuccess.ShouldBeTrue();

        creating.Status.ShouldBe(MatchAssignmentStatus.Expired);
        creating.ClosureReason.ShouldNotBeNull();
    }
}
