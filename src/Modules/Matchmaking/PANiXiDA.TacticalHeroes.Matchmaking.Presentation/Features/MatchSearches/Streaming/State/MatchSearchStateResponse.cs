namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

public sealed record MatchSearchStateResponse(
    Guid? Id,
    string? Status,
    Guid? GameId,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ExpiresAt);
