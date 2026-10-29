using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

public static class MatchmakingPolicy
{
    private const int InitialRatingGap = 100;
    private const int RatingGapStep = 50;
    private const int MaximumRatingGap = 500;

    private static readonly TimeSpan s_expansionInterval = TimeSpan.FromSeconds(15);

    public static IReadOnlyList<MatchmakingPair> SelectPairs(
        IEnumerable<MatchSearch> searches,
        MatchmakingTimestamp now)
    {
        var ordered = searches.OrderBy(search => search.QueuedAt.Value).ThenBy(search => search.Id.Value).ToArray();

        HashSet<MatchSearchId> selected = [];
        List<MatchmakingPair> pairs = [];

        foreach (var first in ordered)
        {
            if (selected.Contains(first.Id))
            {
                continue;
            }

            var second = ordered
                .Where(search => !selected.Contains(search.Id)
                    && CanMatch(first: first, second: search, now: now))
                .OrderBy(search => first.Mode == MatchmakingMode.RankedDuel
                    ? Math.Abs((long)first.Rating.Value - search.Rating.Value)
                    : 0)
                .ThenBy(search => search.QueuedAt.Value)
                .ThenBy(search => search.Id.Value)
                .FirstOrDefault();

            if (second is null)
            {
                continue;
            }

            var pair = MatchmakingPair.Create(first.Id.Value, second.Id.Value);

            if (pair.IsFailure)
            {
                continue;
            }

            selected.Add(first.Id);
            selected.Add(second.Id);
            pairs.Add(pair.Value);
        }

        return pairs;
    }

    public static bool CanMatch(
        MatchSearch first,
        MatchSearch second,
        MatchmakingTimestamp now)
    {
        if (first.Id == second.Id
            || first.PlayerId == second.PlayerId
            || first.Mode != second.Mode
            || first.Status != MatchSearchStatus.Searching
            || second.Status != MatchSearchStatus.Searching
            || first.QueuedAt > now
            || second.QueuedAt > now
            || first.ExpiresAt <= now
            || second.ExpiresAt <= now)
        {
            return false;
        }

        if (first.Mode == MatchmakingMode.CasualDuel)
        {
            return true;
        }

        if (first.Mode != MatchmakingMode.RankedDuel)
        {
            return false;
        }

        var difference = Math.Abs((long)first.Rating.Value - second.Rating.Value);

        return difference <= GetRatingGap(search: first, now: now)
            && difference <= GetRatingGap(search: second, now: now);
    }

    private static long GetRatingGap(MatchSearch search, MatchmakingTimestamp now)
    {
        var steps = (now.Value - search.QueuedAt.Value).Ticks / s_expansionInterval.Ticks;

        return Math.Min(MaximumRatingGap, InitialRatingGap + steps * RatingGapStep);
    }
}
