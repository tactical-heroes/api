namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;

public sealed record BattleCreationRequested(
    Guid GameId,
    Guid FirstPlayerId,
    Guid FirstBuildId,
    Guid SecondPlayerId,
    Guid SecondBuildId) : DomainEvent;
