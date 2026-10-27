using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
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
        search.GameId.ShouldBeNull();
        var domainEvent = search.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<MatchSearchStateChanged>();
        domainEvent.MatchSearchId.ShouldBe(search.Id.Value);
        domainEvent.PlayerId.ShouldBe(search.PlayerId.Value);
        domainEvent.Status.ShouldBe("Searching");
        domainEvent.GameId.ShouldBeNull();
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

    [Fact(DisplayName = "Search should assign game and publish state when search was matched")]
    public void AssignGame_Should_AssignGameAndPublishState_When_SearchWasMatched()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        search.GameId.ShouldBeNull();
        var gameId = GameId.New();
        var count = search.GetDomainEvents().Count;

        search.AssignGame(gameId).IsSuccess.ShouldBeTrue();

        search.GameId.ShouldBe(gameId);
        search.Status.ShouldBe(MatchSearchStatus.Matched);
        search.GetDomainEvents().Count.ShouldBe(count + 1);
        var changed = search.GetDomainEvents().Last().ShouldBeOfType<MatchSearchStateChanged>();
        changed.MatchSearchId.ShouldBe(search.Id.Value);
        changed.PlayerId.ShouldBe(search.PlayerId.Value);
        changed.Status.ShouldBe("Matched");
        changed.GameId.ShouldBe(gameId.Value);
    }

    [Theory(DisplayName = "Search should publish once when same game is assigned again")]
    [InlineData(false)]
    [InlineData(true)]
    public void AssignGame_Should_PublishOnce_When_SameGameIsAssignedAgain(bool closed)
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var gameId = GameId.New();
        search.AssignGame(gameId).IsSuccess.ShouldBeTrue();
        if (closed)
        {
            search.Close().IsSuccess.ShouldBeTrue();
        }

        var status = search.Status;
        var count = search.GetDomainEvents().Count;

        search.AssignGame(gameId).IsSuccess.ShouldBeTrue();

        search.GameId.ShouldBe(gameId);
        search.Status.ShouldBe(status);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Theory(DisplayName = "Search should preserve original game when another game is assigned")]
    [InlineData(false)]
    [InlineData(true)]
    public void AssignGame_Should_PreserveOriginalGame_When_AnotherGameIsAssigned(bool closed)
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var gameId = GameId.New();
        search.AssignGame(gameId).IsSuccess.ShouldBeTrue();
        if (closed)
        {
            search.Close().IsSuccess.ShouldBeTrue();
        }

        var status = search.Status;
        var count = search.GetDomainEvents().Count;

        search.AssignGame(GameId.New()).IsFailure.ShouldBeTrue();

        search.GameId.ShouldBe(gameId);
        search.Status.ShouldBe(status);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Theory(DisplayName = "Search should reject game assignment when search was not matched")]
    [InlineData("Searching")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    [InlineData("Closed")]
    public void AssignGame_Should_RejectGameAssignment_When_SearchWasNotMatched(string status)
    {
        var search = CreateSearch();
        if (status == "Cancelled")
        {
            search.Cancel().IsSuccess.ShouldBeTrue();
        }
        else if (status == "Expired")
        {
            search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }
        else if (status == "Closed")
        {
            search.Matched(At(1)).IsSuccess.ShouldBeTrue();
            search.Close().IsSuccess.ShouldBeTrue();
        }

        var count = search.GetDomainEvents().Count;

        search.AssignGame(GameId.New()).IsFailure.ShouldBeTrue();

        search.GameId.ShouldBeNull();
        search.Status.Name.ShouldBe(status);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should preserve game when search is closed")]
    public void Close_Should_PreserveGame_When_SearchIsClosed()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var gameId = GameId.New();
        search.AssignGame(gameId).IsSuccess.ShouldBeTrue();

        search.Close().IsSuccess.ShouldBeTrue();

        search.GameId.ShouldBe(gameId);
        search.GetDomainEvents().Last().ShouldBeOfType<MatchSearchStateChanged>().GameId.ShouldBe(gameId.Value);
    }

    [Fact(DisplayName = "Search should close and publish state when search was matched")]
    public void Close_Should_CloseAndPublishState_When_SearchWasMatched()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Close().IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Closed);
        search.GetDomainEvents().Count.ShouldBe(count + 1);
        var changed = search.GetDomainEvents().Last().ShouldBeOfType<MatchSearchStateChanged>();
        changed.MatchSearchId.ShouldBe(search.Id.Value);
        changed.PlayerId.ShouldBe(search.PlayerId.Value);
        changed.Status.ShouldBe("Closed");
    }

    [Fact(DisplayName = "Search should publish once when closed repeatedly")]
    public void Close_Should_PublishOnce_When_ClosedRepeatedly()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        search.Close().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Close().IsSuccess.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Closed);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Theory(DisplayName = "Search should preserve state when search was not matched")]
    [InlineData("Searching")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    public void Close_Should_PreserveState_When_SearchWasNotMatched(string status)
    {
        var search = CreateSearch();
        if (status == "Cancelled")
        {
            search.Cancel().IsSuccess.ShouldBeTrue();
        }
        else if (status == "Expired")
        {
            search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }

        var count = search.GetDomainEvents().Count;

        search.Close().IsFailure.ShouldBeTrue();

        search.Status.Name.ShouldBe(status);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should preserve closed state when search was closed")]
    public void Matched_Should_PreserveClosedState_When_SearchWasClosed()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        search.Close().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Matched(At(1)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Closed);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should preserve closed state when search was closed")]
    public void Cancel_Should_PreserveClosedState_When_SearchWasClosed()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        search.Close().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Cancel().IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Closed);
        search.GetDomainEvents().Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Search should preserve closed state when search was closed")]
    public void Expire_Should_PreserveClosedState_When_SearchWasClosed()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        search.Close().IsSuccess.ShouldBeTrue();
        var count = search.GetDomainEvents().Count;

        search.Expire(At(300)).IsFailure.ShouldBeTrue();

        search.Status.ShouldBe(MatchSearchStatus.Closed);
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
