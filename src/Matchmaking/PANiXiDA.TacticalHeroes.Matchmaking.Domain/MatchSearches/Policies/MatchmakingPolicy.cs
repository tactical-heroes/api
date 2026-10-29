using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

public static class MatchmakingPolicy
{
    public static IReadOnlyList<MatchmakingPair> SelectPairs(
        IEnumerable<MatchmakingCandidate> candidates,
        DateTimeOffset now,
        MatchmakingRules rules)
    {
        var ordered = candidates.OrderBy(candidate => candidate.QueuedAt).ThenBy(candidate => candidate.Id).ToArray();
        HashSet<Guid> selected = [];
        List<MatchmakingPair> pairs = [];
        foreach (var first in ordered)
        {
            if (selected.Contains(first.Id))
            {
                continue;
            }

            var second = ordered
                .Where(candidate => !selected.Contains(candidate.Id)
                    && CanMatch(first: first, second: candidate, now: now, rules: rules))
                .OrderBy(candidate => first.Mode == MatchmakingMode.RankedDuel.Name
                    ? Math.Abs((long)first.Rating - candidate.Rating)
                    : 0)
                .ThenBy(candidate => candidate.QueuedAt)
                .ThenBy(candidate => candidate.Id)
                .FirstOrDefault();
            if (second is null)
            {
                continue;
            }

            var pair = MatchmakingPair.Create(first.Id, second.Id);
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
        MatchmakingCandidate first,
        MatchmakingCandidate second,
        DateTimeOffset now,
        MatchmakingRules rules)
    {
        if (first.Id == Guid.Empty
            || second.Id == Guid.Empty
            || first.PlayerId == Guid.Empty
            || second.PlayerId == Guid.Empty
            || first.Id == second.Id
            || first.PlayerId == second.PlayerId
            || first.Mode != second.Mode
            || first.QueuedAt > now
            || second.QueuedAt > now
            || first.ExpiresAt <= now
            || second.ExpiresAt <= now)
        {
            return false;
        }

        if (first.Mode == MatchmakingMode.CasualDuel.Name)
        {
            return true;
        }

        if (first.Mode != MatchmakingMode.RankedDuel.Name)
        {
            return false;
        }

        var difference = Math.Abs((long)first.Rating - second.Rating);
        return difference <= GetRatingGap(candidate: first, now: now, rules: rules)
            && difference <= GetRatingGap(candidate: second, now: now, rules: rules);
    }

    private static long GetRatingGap(MatchmakingCandidate candidate, DateTimeOffset now, MatchmakingRules rules)
    {
        var remaining = (long)rules.MaximumRatingGap - rules.InitialRatingGap;
        var maximumSteps = (remaining + rules.RatingGapStep - 1) / rules.RatingGapStep;
        var steps = Math.Min((now - candidate.QueuedAt).Ticks / rules.ExpansionInterval.Ticks, maximumSteps);
        return Math.Min(rules.MaximumRatingGap, rules.InitialRatingGap + steps * rules.RatingGapStep);
    }
}
