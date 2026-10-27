using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

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
        var domainEvent = search.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<MatchSearchStateChanged>();
        domainEvent.MatchSearchId.ShouldBe(search.Id.Value);
        domainEvent.PlayerId.ShouldBe(search.PlayerId.Value);
        domainEvent.Status.ShouldBe("Searching");
    }

    [Theory(DisplayName = "Search should expire five minutes after queue entry when created")]
    [InlineData("RankedDuel", 0)]
    [InlineData("CasualDuel", 120)]
    public void Create_Should_ExpireFiveMinutesAfterQueueEntry_When_Created(string mode, int queuedAt)
    {
        var queued = At(queuedAt);

        var search = CreateSearch(mode: MatchmakingMode.Create(mode).Value, queuedAt: queuedAt);

        search.QueuedAt.ShouldBe(queued);
        search.ExpiresAt.Value.ShouldBe(queued.Value.AddMinutes(5));
    }

    [Fact(DisplayName = "Search should reject queue entry when expiration would exceed timestamp range")]
    public void Create_Should_RejectQueueEntry_When_ExpirationWouldExceedTimestampRange()
    {
        var template = CreateSearch();
        var queuedAt = MatchmakingTimestamp.Create(DateTimeOffset.MaxValue);

        var result = MatchSearch.Create(
            template.PlayerId,
            template.BuildId,
            template.Rating,
            template.Mode,
            queuedAt);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should accept latest queue entry when expiration fits timestamp range")]
    public void Create_Should_AcceptLatestQueueEntry_When_ExpirationFitsTimestampRange()
    {
        var template = CreateSearch();
        var queuedAt = MatchmakingTimestamp.Create(DateTimeOffset.MaxValue.AddMinutes(-5));

        var result = MatchSearch.Create(
            template.PlayerId,
            template.BuildId,
            template.Rating,
            template.Mode,
            queuedAt);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ExpiresAt.Value.ShouldBe(DateTimeOffset.MaxValue);
    }

    [Theory(DisplayName = "Search should change status and publish state when search is active")]
    [InlineData("RankedDuel", 0)]
    [InlineData("CasualDuel", 299)]
    public void Matched_Should_ChangeStatusAndPublishState_When_SearchIsActive(string mode, int now)
    {
        var search = CreateSearch(mode: MatchmakingMode.Create(mode).Value);
        var count = search.GetDomainEvents().Count;

        search.Matched(At(now)).IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Matched);
        search.GetDomainEvents().Count.ShouldBe(count + 1);
        var changed = search.GetDomainEvents().Last().ShouldBeOfType<MatchSearchStateChanged>();
        changed.MatchSearchId.ShouldBe(search.Id.Value);
        changed.PlayerId.ShouldBe(search.PlayerId.Value);
        changed.Status.ShouldBe("Matched");
    }

    [Theory(DisplayName = "Search should preserve searching state when time is outside search window")]
    [InlineData(-1)]
    [InlineData(300)]
    [InlineData(301)]
    public void Matched_Should_PreserveSearchingState_When_TimeIsOutsideSearchWindow(int now)
    {
        var search = CreateSearch();
        var count = search.GetDomainEvents().Count;

        search.Matched(At(now)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Searching);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should publish once when called repeatedly")]
    public void Matched_Should_PublishOnce_When_CalledRepeatedly()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Matched(At(301)).IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Matched);
        search.GetDomainEvents().Count.ShouldBe(count);
        search.Cancel().IsFailure.ShouldBeTrue();
        search.Expire(At(300)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search should preserve cancelled state when search was cancelled")]
    public void Matched_Should_PreserveCancelledState_When_SearchWasCancelled()
    {
        var search = CreateSearch();
        search.Cancel().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Matched(At(1)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Cancelled);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should preserve expired state when search was expired")]
    public void Matched_Should_PreserveExpiredState_When_SearchWasExpired()
    {
        var search = CreateSearch();
        search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Matched(At(301)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Expired);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should cancel once when requested repeatedly")]
    public void Cancel_Should_CancelOnce_When_RequestedRepeatedly()
    {
        var search = CreateSearch();

        search.Cancel().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;
        search.Cancel().IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Cancelled);
        search.GetDomainEvents().Count.ShouldBe(count);
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
        search.Matched(At(301)).IsFailure.ShouldBeTrue();
    }
}
