namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

public sealed record GameCreationRequested(
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
