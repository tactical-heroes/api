namespace PANiXiDA.TacticalHeroes.Games.Contracts.Games;

public sealed record GameCreated(
    Guid GameId,
    Guid FirstMatchSearchId,
    Guid SecondMatchSearchId) : IntegrationEvent;
