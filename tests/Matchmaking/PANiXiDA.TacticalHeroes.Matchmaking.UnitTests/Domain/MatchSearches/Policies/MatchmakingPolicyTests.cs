using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Policies;

public sealed class MatchmakingPolicyTests
{
    private static readonly MatchmakingTimestamp s_now =
        MatchmakingTimestamp.Create(DateTimeOffset.UnixEpoch.AddDays(1));

    private static readonly MatchmakingRules s_rules =
        MatchmakingRules.Create(100, 50, TimeSpan.FromSeconds(15), 500).Value;

    [Theory(DisplayName = "Policy should choose oldest or closest opponent when mode is selected")]
    [InlineData("CasualDuel", true)]
    [InlineData("RankedDuel", false)]
    public void SelectPairs_Should_ChooseOldestOrClosestOpponent_When_ModeIsSelected(string mode, bool fifo)
    {
        var first = Search(1000, mode, waited: 30);
        var older = Search(1050, mode, waited: 20);
        var closer = Search(1001, mode, waited: 10);

        var pair = MatchmakingPolicy.SelectPairs([closer, older, first], s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(first.Id.Value);
        pair.SecondSearchId.ShouldBe(fifo ? older.Id.Value : closer.Id.Value);
    }

    [Fact(DisplayName = "Policy should consider entire queue when oldest searches have no opponents")]
    public void SelectPairs_Should_ConsiderEntireQueue_When_OldestSearchesHaveNoOpponents()
    {
        var searches = Enumerable.Range(0, 101).Select(index => Search(index * 1000, waited: 60)).ToList();
        var first = Search(1_000_000, waited: 10);
        var second = Search(1_000_010, waited: 5);
        searches.Add(first);
        searches.Add(second);

        var pair = MatchmakingPolicy.SelectPairs(searches, s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(first.Id.Value);
        pair.SecondSearchId.ShouldBe(second.Id.Value);
    }

    [Fact(DisplayName = "Policy should use each search once when many players are waiting")]
    public void SelectPairs_Should_UseEachSearchOnce_When_ManyPlayersAreWaiting()
    {
        var searches = Enumerable.Range(0, 9).Select(index => Search(1000 + index)).ToArray();

        var pairs = MatchmakingPolicy.SelectPairs(searches, s_now, s_rules);

        pairs.Count.ShouldBe(4);
        pairs.SelectMany(pair => new[] { pair.FirstSearchId, pair.SecondSearchId }).Distinct().Count().ShouldBe(8);
    }

    [Fact(DisplayName = "Policy should preserve search state and events when pairs are selected")]
    public void SelectPairs_Should_PreserveSearchStateAndEvents_When_PairsAreSelected()
    {
        var searches = new[] { Search(1000), Search(1000) };
        var events = searches.SelectMany(search => search.GetDomainEvents()).ToArray();

        var pairs = MatchmakingPolicy.SelectPairs(searches, s_now, s_rules);

        pairs.ShouldHaveSingleItem();
        searches.ShouldAllBe(search => search.Status == MatchSearchStatus.Searching);
        searches.SelectMany(search => search.GetDomainEvents()).ShouldBe(events);
    }

    [Theory(DisplayName = "Policy should respect both rating windows when waiting time changes")]
    [InlineData(14, 14, 150, false)]
    [InlineData(15, 15, 150, true)]
    [InlineData(30, 30, 200, true)]
    [InlineData(30, 0, 150, false)]
    [InlineData(299, 299, 500, true)]
    [InlineData(299, 299, 501, false)]
    public void CanMatch_Should_RespectBothRatingWindows_When_WaitingTimeChanges(
        int firstWait, int secondWait, int difference, bool expected)
    {
        var first = Search(1000, waited: firstWait);
        var second = Search(1000 + difference, waited: secondWait);

        var result = MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBe(expected);
    }

    [Theory(DisplayName = "Policy should reject pair when searches are incompatible")]
    [InlineData("SameSearch", false)]
    [InlineData("SameSearch", true)]
    [InlineData("SamePlayer", false)]
    [InlineData("SamePlayer", true)]
    [InlineData("DifferentMode", false)]
    [InlineData("DifferentMode", true)]
    [InlineData("Expired", false)]
    [InlineData("Expired", true)]
    [InlineData("Future", false)]
    [InlineData("Future", true)]
    [InlineData("Cancelled", false)]
    [InlineData("Cancelled", true)]
    [InlineData("Matched", false)]
    [InlineData("Matched", true)]
    [InlineData("Closed", false)]
    [InlineData("Closed", true)]
    [InlineData("ExpiredStatus", false)]
    [InlineData("ExpiredStatus", true)]
    public void CanMatch_Should_RejectPair_When_SearchesAreIncompatible(string scenario, bool reverse)
    {
        var first = Search(1000);
        var second = scenario switch
        {
            "SameSearch" => first,
            "SamePlayer" => Search(1000, playerId: first.PlayerId),
            "DifferentMode" => Search(1000, mode: "CasualDuel"),
            "Expired" or "ExpiredStatus" => Search(1000, waited: 300),
            "Future" => Search(1000, waited: -1),
            _ => Search(1000)
        };

        if (scenario == "Cancelled")
        {
            second.Cancel().IsSuccess.ShouldBeTrue();
        }
        else if (scenario is "Matched" or "Closed")
        {
            second.Matched(s_now).IsSuccess.ShouldBeTrue();

            if (scenario == "Closed")
            {
                second.Close().IsSuccess.ShouldBeTrue();
            }
        }
        else if (scenario == "ExpiredStatus")
        {
            second.Expire(s_now).IsSuccess.ShouldBeTrue();
        }

        var result = reverse
            ? MatchmakingPolicy.CanMatch(second, first, s_now, s_rules)
            : MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBeFalse();
    }

    [Theory(DisplayName = "Policy should handle extreme ratings when comparing players")]
    [InlineData("RankedDuel", false)]
    [InlineData("CasualDuel", true)]
    public void CanMatch_Should_HandleExtremeRatings_When_ComparingPlayers(string mode, bool expected)
    {
        var first = Search(int.MinValue, mode);
        var second = Search(int.MaxValue, mode);

        var result = MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "Policy should break ties by id when queue times and ratings are equal")]
    public void SelectPairs_Should_BreakTiesById_When_QueueTimesAndRatingsAreEqual()
    {
        var searches = Enumerable.Range(0, 3).Select(_ => Search(1000)).OrderBy(search => search.Id.Value).ToArray();

        var pair = MatchmakingPolicy.SelectPairs(searches.Reverse(), s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(searches[0].Id.Value);
        pair.SecondSearchId.ShouldBe(searches[1].Id.Value);
    }

    private static MatchSearch Search(
        int rating, string mode = "RankedDuel", int waited = 0, PlayerId? playerId = null)
    {
        return MatchSearch.Create(
            playerId ?? PlayerId.New(),
            BuildId.New(),
            Rating.Create(rating),
            MatchmakingMode.Create(mode).Value,
            MatchmakingTimestamp.Create(s_now.Value.AddSeconds(-waited))).Value;
    }
}
