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
        var domainEvent = search.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<MatchSearchStateChanged>();
        domainEvent.MatchSearchId.ShouldBe(search.Id.Value);
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

    [Theory(DisplayName = "Search should close both searches and emit one complete event when pair is valid")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public void MatchWith_Should_CloseBothSearchesAndEmitOneCompleteEvent_When_PairIsValid(string mode)
    {
        var selectedMode = MatchmakingMode.Create(mode).Value;
        var first = CreateSearch(mode: selectedMode);
        var second = CreateSearch(mode: selectedMode);

        first.MatchWith(second, At(1)).IsSuccess.ShouldBeTrue();

        first.Status.ShouldBe(MatchSearchStatus.Matched);
        second.Status.ShouldBe(MatchSearchStatus.Matched);
        var found = first.GetDomainEvents().OfType<MatchFound>().ShouldHaveSingleItem();
        second.GetDomainEvents().OfType<MatchFound>().ShouldBeEmpty();
        found.FirstMatchSearchId.ShouldBe(first.Id.Value);
        found.FirstPlayerId.ShouldBe(first.PlayerId.Value);
        found.FirstBuildId.ShouldBe(first.BuildId.Value);
        found.FirstRating.ShouldBe(first.Rating.Value);
        found.SecondMatchSearchId.ShouldBe(second.Id.Value);
        found.SecondPlayerId.ShouldBe(second.PlayerId.Value);
        found.SecondBuildId.ShouldBe(second.BuildId.Value);
        found.SecondRating.ShouldBe(second.Rating.Value);
        found.Mode.ShouldBe(first.Mode.Name);
        found.MatchedAt.ShouldBe(At(1).Value);
        first.GetDomainEvents().OfType<MatchSearchStateChanged>().Last().Status.ShouldBe("Matched");
        second.GetDomainEvents().OfType<MatchSearchStateChanged>().Last().Status.ShouldBe("Matched");
    }

    [Fact(DisplayName = "Search should preserve both searches when pair is incompatible")]
    public void MatchWith_Should_PreserveBothSearches_When_PairIsIncompatible()
    {
        var first = CreateSearch();
        var samePlayer = CreateSearch(first.PlayerId.Value);
        var otherMode = CreateSearch(mode: MatchmakingMode.CasualDuel);

        first.MatchWith(first, At(1)).IsFailure.ShouldBeTrue();
        first.MatchWith(samePlayer, At(1)).IsFailure.ShouldBeTrue();
        first.MatchWith(otherMode, At(1)).IsFailure.ShouldBeTrue();

        first.Status.ShouldBe(MatchSearchStatus.Searching);
        samePlayer.Status.ShouldBe(MatchSearchStatus.Searching);
        otherMode.Status.ShouldBe(MatchSearchStatus.Searching);
        first.GetDomainEvents().Count.ShouldBe(1);
        samePlayer.GetDomainEvents().Count.ShouldBe(1);
        otherMode.GetDomainEvents().Count.ShouldBe(1);
    }

    [Theory(DisplayName = "Search should preserve both searches when either search is outside its window")]
    [InlineData(-1, 300)]
    [InlineData(300, 300)]
    [InlineData(1, 1)]
    public void MatchWith_Should_PreserveBothSearches_When_EitherSearchIsOutsideItsWindow(int now, int secondExpiry)
    {
        var first = CreateSearch();
        var second = CreateSearch(expiresAt: secondExpiry);

        first.MatchWith(second, At(now)).IsFailure.ShouldBeTrue();
        second.MatchWith(first, At(now)).IsFailure.ShouldBeTrue();

        first.Status.ShouldBe(MatchSearchStatus.Searching);
        second.Status.ShouldBe(MatchSearchStatus.Searching);
        first.GetDomainEvents().Count.ShouldBe(1);
        second.GetDomainEvents().Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Search should reject matching without changing peer when search was cancelled")]
    public void MatchWith_Should_RejectMatchingWithoutChangingPeer_When_SearchWasCancelled()
    {
        var first = CreateSearch();
        var second = CreateSearch();
        first.Cancel().IsSuccess.ShouldBeTrue();

        first.MatchWith(second, At(1)).IsFailure.ShouldBeTrue();
        second.MatchWith(first, At(1)).IsFailure.ShouldBeTrue();

        second.Status.ShouldBe(MatchSearchStatus.Searching);
        second.GetDomainEvents().Count.ShouldBe(1);
        first.GetDomainEvents().OfType<MatchFound>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Search should reject reuse and preserve the original event when search is already matched")]
    public void MatchWith_Should_RejectReuseAndPreserveTheOriginalEvent_When_SearchIsAlreadyMatched()
    {
        var first = CreateSearch();
        var second = CreateSearch();
        var third = CreateSearch();
        first.MatchWith(second, At(1)).IsSuccess.ShouldBeTrue();
        var found = first.GetDomainEvents().OfType<MatchFound>().ShouldHaveSingleItem();
        var count = first.GetDomainEvents().Count;

        first.MatchWith(second, At(2)).IsFailure.ShouldBeTrue();
        second.MatchWith(first, At(2)).IsFailure.ShouldBeTrue();
        third.MatchWith(first, At(2)).IsFailure.ShouldBeTrue();
        first.Cancel().IsFailure.ShouldBeTrue();
        first.Expire(At(300)).IsFailure.ShouldBeTrue();

        third.Status.ShouldBe(MatchSearchStatus.Searching);
        first.GetDomainEvents().Count.ShouldBe(count);
        first.GetDomainEvents().OfType<MatchFound>().ShouldHaveSingleItem().Id.ShouldBe(found.Id);
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
        search.MatchWith(CreateSearch(), At(301)).IsFailure.ShouldBeTrue();
    }
}
