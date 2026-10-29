using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Policies;

public sealed class MatchmakingPolicyTests
{
    private static readonly DateTimeOffset s_now = DateTimeOffset.UnixEpoch.AddDays(1);

    private static readonly MatchmakingRules s_rules =
        MatchmakingRules.Create(100, 50, TimeSpan.FromSeconds(15), 500).Value;

    [Theory(DisplayName = "Policy should choose oldest or closest opponent when mode is selected")]
    [InlineData("CasualDuel", true)]
    [InlineData("RankedDuel", false)]
    public void SelectPairs_Should_ChooseOldestOrClosestOpponent_When_ModeIsSelected(string mode, bool fifo)
    {
        var first = Candidate(1000, mode, waited: 30);
        var older = Candidate(1050, mode, waited: 20);
        var closer = Candidate(1001, mode, waited: 10);

        var pair = MatchmakingPolicy.SelectPairs([closer, older, first], s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(first.Id);
        pair.SecondSearchId.ShouldBe(fifo ? older.Id : closer.Id);
    }

    [Fact(DisplayName = "Policy should consider entire queue when oldest searches have no opponents")]
    public void SelectPairs_Should_ConsiderEntireQueue_When_OldestSearchesHaveNoOpponents()
    {
        var searches = Enumerable.Range(0, 101).Select(index => Candidate(index * 1000, waited: 60)).ToList();
        var first = Candidate(1_000_000, waited: 10);
        var second = Candidate(1_000_010, waited: 5);
        searches.Add(first);
        searches.Add(second);

        var pair = MatchmakingPolicy.SelectPairs(searches, s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(first.Id);
        pair.SecondSearchId.ShouldBe(second.Id);
    }

    [Fact(DisplayName = "Policy should use each search once when many players are waiting")]
    public void SelectPairs_Should_UseEachSearchOnce_When_ManyPlayersAreWaiting()
    {
        var searches = Enumerable.Range(0, 9).Select(index => Candidate(1000 + index)).ToArray();

        var pairs = MatchmakingPolicy.SelectPairs(searches, s_now, s_rules);

        pairs.Count.ShouldBe(4);
        pairs.SelectMany(pair => new[] { pair.FirstSearchId, pair.SecondSearchId }).Distinct().Count().ShouldBe(8);
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
        var first = Candidate(1000, waited: firstWait);
        var second = Candidate(1000 + difference, waited: secondWait);

        var result = MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBe(expected);
    }

    [Theory(DisplayName = "Policy should reject pair when candidates are incompatible")]
    [InlineData("SameSearch")]
    [InlineData("SamePlayer")]
    [InlineData("EmptySearchId")]
    [InlineData("EmptyPlayerId")]
    [InlineData("DifferentMode")]
    [InlineData("Expired")]
    [InlineData("Future")]
    [InlineData("UnknownMode")]
    public void CanMatch_Should_RejectPair_When_CandidatesAreIncompatible(string scenario)
    {
        var first = Candidate(1000);
        var second = Candidate(1000);
        if (scenario == "UnknownMode")
        {
            first = first with { Mode = "Unknown" };
        }

        second = scenario switch
        {
            "SameSearch" => second with { Id = first.Id },
            "SamePlayer" => second with { PlayerId = first.PlayerId },
            "EmptySearchId" => second with { Id = Guid.Empty },
            "EmptyPlayerId" => second with { PlayerId = Guid.Empty },
            "DifferentMode" => second with { Mode = "CasualDuel" },
            "Expired" => second with { ExpiresAt = s_now },
            "Future" => second with { QueuedAt = s_now.AddSeconds(1) },
            _ => second with { Mode = "Unknown" }
        };

        var result = MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBeFalse();
    }

    [Theory(DisplayName = "Policy should handle extreme ratings when comparing players")]
    [InlineData("RankedDuel", false)]
    [InlineData("CasualDuel", true)]
    public void CanMatch_Should_HandleExtremeRatings_When_ComparingPlayers(string mode, bool expected)
    {
        var first = Candidate(int.MinValue, mode);
        var second = Candidate(int.MaxValue, mode);

        var result = MatchmakingPolicy.CanMatch(first, second, s_now, s_rules);

        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "Policy should break ties by id when queue times and ratings are equal")]
    public void SelectPairs_Should_BreakTiesById_When_QueueTimesAndRatingsAreEqual()
    {
        var first = Candidate(1000) with { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") };
        var second = Candidate(1000) with { Id = Guid.Parse("00000000-0000-0000-0000-000000000002") };
        var third = Candidate(1000) with { Id = Guid.Parse("00000000-0000-0000-0000-000000000003") };

        var pair = MatchmakingPolicy.SelectPairs([third, second, first], s_now, s_rules).ShouldHaveSingleItem();

        pair.FirstSearchId.ShouldBe(first.Id);
        pair.SecondSearchId.ShouldBe(second.Id);
    }

    private static MatchmakingCandidate Candidate(int rating, string mode = "RankedDuel", int waited = 0)
    {
        return new MatchmakingCandidate(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            rating,
            mode,
            s_now.AddSeconds(-waited),
            s_now.AddSeconds(300 - waited));
    }
}
