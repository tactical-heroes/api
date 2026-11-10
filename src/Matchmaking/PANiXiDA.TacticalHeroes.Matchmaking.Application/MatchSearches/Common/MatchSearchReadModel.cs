namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common;

public sealed record MatchSearchReadModel(
    Guid? Id,
    string? Status,
    Guid? GameId,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ExpiresAt) : IReadModel;
