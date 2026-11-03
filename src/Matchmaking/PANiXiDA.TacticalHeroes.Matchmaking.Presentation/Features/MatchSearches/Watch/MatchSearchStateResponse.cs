namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public sealed record MatchSearchStateResponse(
    long Version,
    Guid? MatchSearchId,
    string? Status,
    Guid? GameId,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive);
