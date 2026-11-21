namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;

public sealed record BattleCreated(Guid GameId, Guid BattleId) : DomainEvent;
