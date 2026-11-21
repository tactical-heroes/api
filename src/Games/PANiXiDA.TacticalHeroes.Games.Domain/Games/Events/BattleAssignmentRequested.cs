namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

public sealed record BattleAssignmentRequested(Guid GameId, Guid BattleId) : DomainEvent;
