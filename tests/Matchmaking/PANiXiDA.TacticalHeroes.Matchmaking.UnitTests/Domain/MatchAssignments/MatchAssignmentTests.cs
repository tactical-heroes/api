using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments;

public sealed class MatchAssignmentTests
{
    [Fact(DisplayName = "Assignment should capture selections and request game creation when pair is matched")]
    public void Create_Should_CaptureSelectionsAndRequestGameCreation_When_PairIsMatched()
    {
        var first = Ticket();
        var second = Ticket();

        var assignment = MatchAssignment.Create(first, second, At(1), At(60)).Value;

        assignment.Participants.Count.ShouldBe(2);
        assignment.Participants.Select(value => value.Id).Distinct().Count().ShouldBe(2);
        assignment.Participants.ShouldAllBe(value => value.MatchAssignmentId == assignment.Id);
        var participant = assignment.Participants.Single(value => value.TicketId == first.Id);
        participant.PlayerId.ShouldBe(first.PlayerId);
        participant.BuildSnapshotId.ShouldBe(first.BuildSnapshotId);
        participant.Rating.ShouldBe(first.Rating);
        first.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        second.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        assignment.MatchedAt.ShouldBe(At(1));
        assignment.CreationDeadline.ShouldBe(At(60));
        assignment.Status.ShouldBe(MatchAssignmentStatus.CreatingGame);
        var state = assignment.GetDomainEvents().OfType<MatchAssignmentStateChanged>().ShouldHaveSingleItem();
        state.Status.ShouldBe(MatchAssignmentStatus.CreatingGame.Name);
        var request = assignment.GetDomainEvents().OfType<GameCreationRequested>().ShouldHaveSingleItem();
        request.MatchAssignmentId.ShouldBe(assignment.Id.Value);
        Should.Throw<NotSupportedException>(() => ((ICollection<MatchParticipant>)assignment.Participants).Clear());
    }

    [Fact(DisplayName = "Assignment should reject incompatible tickets when created")]
    public void Create_Should_RejectIncompatibleTickets_When_Created()
    {
        var first = Ticket();

        MatchAssignment.Create(first, first, At(1), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(first.PlayerId.Value), At(1), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(mode: "other"), At(1), At(60)).IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "Assignment should reject deadline when it does not follow match time")]
    [InlineData(1)]
    [InlineData(0)]
    public void Create_Should_RejectDeadline_When_ItDoesNotFollowMatchTime(int creation)
    {
        var result = MatchAssignment.Create(Ticket(), Ticket(), At(1), At(creation));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Assignment should reject unavailable tickets when created")]
    public void Create_Should_RejectUnavailableTickets_When_Created()
    {
        var first = Ticket();
        var second = Ticket();

        MatchAssignment.Create(first, second, At(-1), At(60)).IsFailure.ShouldBeTrue();
        MatchAssignment.Create(first, Ticket(expiresAt: 1), At(1), At(60)).IsFailure.ShouldBeTrue();
        second.Reserve(MatchAssignmentId.New(), At(1)).IsSuccess.ShouldBeTrue();
        MatchAssignment.Create(first, second, At(2), At(60)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Assignment should bind exactly one game when delivery is retried")]
    public void Complete_Should_BindExactlyOneGame_When_DeliveryIsRetried()
    {
        var assignment = Assignment();
        var game = GameId.Create(Guid.CreateVersion7()).Value;

        assignment.Complete(game, At(3)).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Complete(game, At(100)).IsSuccess.ShouldBeTrue();
        assignment.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(4)).IsFailure.ShouldBeTrue();

        assignment.GameId.ShouldBe(game);
        assignment.Status.ShouldBe(MatchAssignmentStatus.Ready);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        assignment.GetDomainEvents().OfType<GameCreationRequested>().ShouldHaveSingleItem();
        assignment.Expire(At(100)).IsFailure.ShouldBeTrue();
        assignment.Fail().IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Assignment should reject result when outside creation window")]
    public void Complete_Should_RejectResult_When_OutsideCreationWindow()
    {
        var game = GameId.Create(Guid.CreateVersion7()).Value;
        var assignment = Assignment();

        assignment.Complete(game, At(0)).IsFailure.ShouldBeTrue();
        assignment.Complete(game, At(60)).IsFailure.ShouldBeTrue();
        assignment.Expire(At(60)).IsSuccess.ShouldBeTrue();
        assignment.Complete(game, At(61)).IsFailure.ShouldBeTrue();

        assignment.GameId.ShouldBeNull();
    }

    [Fact(DisplayName = "Assignment should close game creation once when retried")]
    public void Fail_Should_CloseGameCreationOnce_When_Retried()
    {
        var assignment = Assignment();

        assignment.Fail().IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Fail().IsSuccess.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.Failed);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        assignment.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(3)).IsFailure.ShouldBeTrue();
        assignment.Expire(At(100)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Assignment should expire creation and publish once when overdue")]
    public void Expire_Should_ExpireCreationAndPublishOnce_When_Overdue()
    {
        var assignment = Assignment();
        assignment.Expire(At(59)).IsFailure.ShouldBeTrue();

        assignment.Expire(At(60)).IsSuccess.ShouldBeTrue();
        var count = assignment.GetDomainEvents().Count;
        assignment.Expire(At(61)).IsSuccess.ShouldBeTrue();

        assignment.Status.ShouldBe(MatchAssignmentStatus.Expired);
        assignment.GetDomainEvents().Count.ShouldBe(count);
        assignment.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(61)).IsFailure.ShouldBeTrue();
        assignment.Fail().IsFailure.ShouldBeTrue();
    }
}
