namespace PANiXiDA.TacticalHeroes.Games.Contracts.Games;

public sealed record GameCreated(
    Guid GameId,
    Guid FirstMatchSearchId,
    Guid SecondMatchSearchId,
    Guid FirstPlayerId,
    Guid FirstBuildId,
    Guid SecondPlayerId,
    Guid SecondBuildId) : IntegrationEvent;
