namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

public sealed record MatchFound(
    Guid FirstMatchSearchId,
    Guid FirstPlayerId,
    Guid FirstBuildId,
    int FirstRating,
    Guid SecondMatchSearchId,
    Guid SecondPlayerId,
    Guid SecondBuildId,
    int SecondRating,
    string Mode,
    DateTimeOffset MatchedAt) : DomainEvent;
