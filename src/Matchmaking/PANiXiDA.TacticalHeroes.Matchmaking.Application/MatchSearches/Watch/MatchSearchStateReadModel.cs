using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed record MatchSearchStateReadModel(
    long Version,
    Guid? MatchSearchId,
    string? Status,
    Guid? GameId,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? ExpiresAt) : IReadModel
{
    public bool IsActive => Status == MatchSearchStatus.Searching.Name || Status == MatchSearchStatus.Matched.Name;
}
