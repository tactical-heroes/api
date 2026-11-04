namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed record MatchSearchStateReadModel(
    Guid? MatchSearchId,
    string? Status,
    Guid? GameId,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ExpiresAt) : IReadModel;
