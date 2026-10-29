namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

public sealed record MatchmakingCandidate(
    Guid Id,
    Guid PlayerId,
    int Rating,
    string Mode,
    DateTimeOffset QueuedAt,
    DateTimeOffset ExpiresAt);
