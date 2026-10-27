namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

public sealed record StartMatchSearchResult(Guid Id, string Status, DateTimeOffset QueuedAt, DateTimeOffset ExpiresAt);
