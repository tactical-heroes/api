using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches;

public sealed class MatchSearchTests
{
    [Fact(DisplayName = "Search should capture selection and announce searching when created")]
    public void Create_Should_CaptureSelectionAndAnnounceSearching_When_Created()
    {
        var search = CreateSearch();

        search.Id.Value.ShouldNotBe(Guid.Empty);
        search.Status.ShouldBe(MatchSearchStatus.Searching);
        search.MatchId.ShouldBeNull();
        var domainEvent = search.GetDomainEvents()
            .ShouldHaveSingleItem()
            .ShouldBeOfType<MatchSearchStateChanged>();
        domainEvent.PlayerId.ShouldBe(search.PlayerId.Value);
        domainEvent.Status.ShouldBe("Searching");
    }

    [Theory(DisplayName = "Search should reject invalid window when created")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_Should_RejectInvalidWindow_When_Created(int end)
    {
        var template = CreateSearch();

        var result = MatchSearch.Create(
            template.PlayerId,
            template.BuildId,
            template.Rating,
            template.Mode,
            At(0),
            At(end));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should be idempotent and exclusive when repeated")]
    public void Reserve_Should_BeIdempotentAndExclusive_When_Repeated()
    {
        var search = CreateSearch();
        var matchId = MatchId.New();

        search.Reserve(matchId, At(1)).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;
        search.Reserve(matchId, At(301)).IsSuccess.ShouldBeTrue();
        search.Reserve(MatchId.New(), At(2)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Reserved);
        search.MatchId.ShouldBe(matchId);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Theory(DisplayName = "Search should preserve state when time is outside window")]
    [InlineData(-1)]
    [InlineData(300)]
    [InlineData(301)]
    public void Reserve_Should_PreserveState_When_TimeIsOutsideWindow(int now)
    {
        var search = CreateSearch();

        search.Reserve(MatchId.New(), At(now)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Searching);
        search.MatchId.ShouldBeNull();
        search.GetDomainEvents().Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Search should reject default identifier when requested")]
    public void Reserve_Should_RejectDefaultIdentifier_When_Requested()
    {
        var search = CreateSearch();

        search.Reserve(default, At(1)).IsFailure.ShouldBeTrue();

        search.MatchId.ShouldBeNull();
    }

    [Fact(DisplayName = "Search should require owning reservation and publish once when repeated")]
    public void MarkAssigned_Should_RequireOwningReservationAndPublishOnce_When_Repeated()
    {
        var search = CreateSearch();
        var matchId = MatchId.New();
        search.MarkAssigned(matchId).IsFailure.ShouldBeTrue();
        search.Reserve(matchId, At(1)).IsSuccess.ShouldBeTrue();

        search.MarkAssigned(MatchId.New()).IsFailure.ShouldBeTrue();
        search.MarkAssigned(matchId).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;
        search.MarkAssigned(matchId).IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Assigned);
        search.GetDomainEvents().Count.ShouldBe(count);
        search.Cancel().IsFailure.ShouldBeTrue();
        search.Reserve(matchId, At(2)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should retain reservation when reserved search is cancelled")]
    public void Cancel_Should_RetainReservation_When_ReservedSearchIsCancelled()
    {
        var search = CreateSearch();
        var matchId = MatchId.New();
        search.Reserve(matchId, At(1)).IsSuccess.ShouldBeTrue();

        search.Cancel().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;
        search.Cancel().IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Cancelled);
        search.MatchId.ShouldBe(matchId);
        search.GetDomainEvents().Count.ShouldBe(count);
        search.MarkAssigned(matchId).IsFailure.ShouldBeTrue();
        search.Reserve(MatchId.New(), At(2)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should terminate searching search when requested")]
    public void Cancel_Should_TerminateSearchingSearch_When_Requested()
    {
        var search = CreateSearch();

        search.Cancel().IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Cancelled);
        search.Expire(At(300)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should respect deadline and publish once when repeated")]
    public void Expire_Should_RespectDeadlineAndPublishOnce_When_Repeated()
    {
        var search = CreateSearch();

        search.Expire(At(299)).IsFailure.ShouldBeTrue();
        search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;
        search.Expire(At(301)).IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Expired);
        search.GetDomainEvents().Count.ShouldBe(count);
        search.Cancel().IsFailure.ShouldBeTrue();
        search.Reserve(MatchId.New(), At(301)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should leave reserved search unchanged when search deadline elapses")]
    public void Expire_Should_LeaveReservedSearchUnchanged_When_SearchDeadlineElapses()
    {
        var search = CreateSearch();
        search.Reserve(MatchId.New(), At(1)).IsSuccess.ShouldBeTrue();

        search.Expire(At(300)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Reserved);
    }
}
