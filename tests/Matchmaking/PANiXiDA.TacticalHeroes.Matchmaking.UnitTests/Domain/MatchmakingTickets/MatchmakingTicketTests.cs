using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Events;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchmakingTickets;

public sealed class MatchmakingTicketTests
{
    [Fact(DisplayName = "Ticket should capture selection and announce searching when created")]
    public void Create_Should_CaptureSelectionAndAnnounceSearching_When_Created()
    {
        var ticket = Ticket();

        ticket.Id.Value.ShouldNotBe(Guid.Empty);
        ticket.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        ticket.MatchAssignmentId.ShouldBeNull();
        var domainEvent = ticket.GetDomainEvents()
            .ShouldHaveSingleItem()
            .ShouldBeOfType<MatchmakingTicketStateChanged>();
        domainEvent.PlayerId.ShouldBe(ticket.PlayerId.Value);
        domainEvent.Status.ShouldBe("Searching");
    }

    [Theory(DisplayName = "Ticket should reject invalid window when created")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_Should_RejectInvalidWindow_When_Created(int end)
    {
        var template = Ticket();

        var result = MatchmakingTicket.Create(
            template.PlayerId,
            template.BuildId,
            template.Rating,
            template.Mode,
            At(0),
            At(end));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Ticket should be idempotent and exclusive when repeated")]
    public void Reserve_Should_BeIdempotentAndExclusive_When_Repeated()
    {
        var ticket = Ticket();
        var assignmentId = MatchAssignmentId.New();

        ticket.Reserve(assignmentId, At(1)).IsSuccess.ShouldBeTrue();
        var count = ticket.GetDomainEvents().Count;
        ticket.Reserve(assignmentId, At(301)).IsSuccess.ShouldBeTrue();
        ticket.Reserve(MatchAssignmentId.New(), At(2)).IsFailure.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Reserved);
        ticket.MatchAssignmentId.ShouldBe(assignmentId);
        ticket.GetDomainEvents().Count.ShouldBe(count);
    }

    [Theory(DisplayName = "Ticket should preserve state when time is outside window")]
    [InlineData(-1)]
    [InlineData(300)]
    [InlineData(301)]
    public void Reserve_Should_PreserveState_When_TimeIsOutsideWindow(int now)
    {
        var ticket = Ticket();

        ticket.Reserve(MatchAssignmentId.New(), At(now)).IsFailure.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        ticket.MatchAssignmentId.ShouldBeNull();
        ticket.GetDomainEvents().Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Ticket should reject default identifier when requested")]
    public void Reserve_Should_RejectDefaultIdentifier_When_Requested()
    {
        var ticket = Ticket();

        ticket.Reserve(default, At(1)).IsFailure.ShouldBeTrue();

        ticket.MatchAssignmentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Ticket should require owning reservation and publish once when repeated")]
    public void MarkAssigned_Should_RequireOwningReservationAndPublishOnce_When_Repeated()
    {
        var ticket = Ticket();
        var assignmentId = MatchAssignmentId.New();
        ticket.MarkAssigned(assignmentId).IsFailure.ShouldBeTrue();
        ticket.Reserve(assignmentId, At(1)).IsSuccess.ShouldBeTrue();

        ticket.MarkAssigned(MatchAssignmentId.New()).IsFailure.ShouldBeTrue();
        ticket.MarkAssigned(assignmentId).IsSuccess.ShouldBeTrue();
        var count = ticket.GetDomainEvents().Count;
        ticket.MarkAssigned(assignmentId).IsSuccess.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Assigned);
        ticket.GetDomainEvents().Count.ShouldBe(count);
        ticket.Cancel().IsFailure.ShouldBeTrue();
        ticket.Reserve(assignmentId, At(2)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Ticket should retain reservation when reserved ticket is cancelled")]
    public void Cancel_Should_RetainReservation_When_ReservedTicketIsCancelled()
    {
        var ticket = Ticket();
        var assignmentId = MatchAssignmentId.New();
        ticket.Reserve(assignmentId, At(1)).IsSuccess.ShouldBeTrue();

        ticket.Cancel().IsSuccess.ShouldBeTrue();
        var count = ticket.GetDomainEvents().Count;
        ticket.Cancel().IsSuccess.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Cancelled);
        ticket.MatchAssignmentId.ShouldBe(assignmentId);
        ticket.GetDomainEvents().Count.ShouldBe(count);
        ticket.MarkAssigned(assignmentId).IsFailure.ShouldBeTrue();
        ticket.Reserve(MatchAssignmentId.New(), At(2)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Ticket should terminate searching ticket when requested")]
    public void Cancel_Should_TerminateSearchingTicket_When_Requested()
    {
        var ticket = Ticket();

        ticket.Cancel().IsSuccess.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Cancelled);
        ticket.Expire(At(300)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Ticket should respect deadline and publish once when repeated")]
    public void Expire_Should_RespectDeadlineAndPublishOnce_When_Repeated()
    {
        var ticket = Ticket();

        ticket.Expire(At(299)).IsFailure.ShouldBeTrue();
        ticket.Expire(At(300)).IsSuccess.ShouldBeTrue();
        var count = ticket.GetDomainEvents().Count;
        ticket.Expire(At(301)).IsSuccess.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Expired);
        ticket.GetDomainEvents().Count.ShouldBe(count);
        ticket.Cancel().IsFailure.ShouldBeTrue();
        ticket.Reserve(MatchAssignmentId.New(), At(301)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Ticket should leave reserved ticket unchanged when search deadline elapses")]
    public void Expire_Should_LeaveReservedTicketUnchanged_When_SearchDeadlineElapses()
    {
        var ticket = Ticket();
        ticket.Reserve(MatchAssignmentId.New(), At(1)).IsSuccess.ShouldBeTrue();

        ticket.Expire(At(300)).IsFailure.ShouldBeTrue();

        ticket.Status.ShouldBe(MatchmakingTicketStatus.Reserved);
    }
}
