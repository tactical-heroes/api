namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;

public sealed record StartMatchSearchResponse(
    Guid Id,
    string Status,
    DateTimeOffset QueuedAt,
    DateTimeOffset ExpiresAt);
