namespace PANiXiDA.TacticalHeroes.Battle.Contracts.Battles;

public sealed record BattleCreated(Guid GameId, Guid BattleId) : IntegrationEvent;
