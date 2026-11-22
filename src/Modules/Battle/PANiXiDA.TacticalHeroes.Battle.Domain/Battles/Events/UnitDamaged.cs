namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;

public sealed record UnitDamaged(
    Guid BattleId,
    Guid AttackerId,
    Guid DefenderId,
    long CalculatedDamage,
    long AppliedDamage,
    int KilledCount,
    int RemainingCount,
    int CurrentUnitHealth) : DomainEvent;
