namespace PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches;

public sealed record MatchFound(
    Guid Id,
    DateTimeOffset OccurredOnUtc,
    Guid FirstMatchSearchId,
    Guid FirstPlayerId,
    Guid FirstBuildId,
    int FirstRating,
    Guid SecondMatchSearchId,
    Guid SecondPlayerId,
    Guid SecondBuildId,
    int SecondRating,
    string Mode,
    DateTimeOffset MatchedAt);
