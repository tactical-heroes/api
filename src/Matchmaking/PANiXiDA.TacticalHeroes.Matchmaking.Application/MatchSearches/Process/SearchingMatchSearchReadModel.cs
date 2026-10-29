namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

public sealed record SearchingMatchSearchReadModel(
    Guid Id,
    Guid PlayerId,
    int Rating,
    string Mode,
    DateTimeOffset QueuedAt,
    DateTimeOffset ExpiresAt) : IReadModel;
