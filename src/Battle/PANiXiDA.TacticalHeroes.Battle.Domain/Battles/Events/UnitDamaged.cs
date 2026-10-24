using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;

public sealed record UnitDamaged(
    BattleId BattleId,
    UnitId AttackerId,
    UnitId DefenderId,
    DamageResult Result) : DomainEvent;
