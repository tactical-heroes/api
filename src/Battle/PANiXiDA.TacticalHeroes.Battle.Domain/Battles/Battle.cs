using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

public sealed class Battle : AggregateRoot<BattleId>
{
    private readonly List<Unit> _units = [];

    private Battle(BattleId id) : base(id)
    {
    }

    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();

    public static Battle Create()
    {
        return new Battle(BattleId.New());
    }

    public Result AddUnits(IEnumerable<Unit> units)
    {
        var participants = units.ToArray();
        if (participants.Any(unit => unit is null || unit.BattleId != Id)
            || participants.Select(unit => unit.Id).Distinct().Count() != participants.Length
            || participants.Any(unit => _units.Any(existing => existing.Id == unit.Id)))
        {
            return Result.Failure(Error.Validation(
                "Units must belong to this battle and have distinct, new identifiers."));
        }

        _units.AddRange(participants);

        return Result.Success();
    }

    public Result Attack(UnitId attackerId, UnitId defenderId)
    {
        if (attackerId == defenderId)
        {
            return Result.Failure(Error.Validation("A unit cannot attack itself."));
        }

        var attacker = _units.SingleOrDefault(unit => unit.Id == attackerId);
        var defender = _units.SingleOrDefault(unit => unit.Id == defenderId);
        if (attacker is null || defender is null || !attacker.IsAlive || !defender.IsAlive)
        {
            return Result.Failure(Error.Validation(
                "Both participants must be living units in this battle."));
        }

        var damage = DamageCalculator.Calculate(
            attackerStats: attacker.Stats,
            attackerStack: attacker.StackState,
            damageRange: attacker.DamageRange,
            defenderStats: defender.Stats,
            roll: attacker.DamageRange.Roll());
        if (damage.IsFailure)
        {
            return Result.Failure(damage.Errors);
        }

        var previousCount = defender.Count;
        var previousHealth = defender.TotalHealth;
        var result = defender.TakeDamage(damage.Value);
        var appliedDamage = previousHealth - defender.TotalHealth;
        if (result.IsSuccess && appliedDamage > 0)
        {
            AddDomainEvent(new UnitDamaged(
                BattleId: Id.Value,
                AttackerId: attackerId.Value,
                DefenderId: defenderId.Value,
                CalculatedDamage: damage.Value.Value,
                AppliedDamage: appliedDamage,
                KilledCount: previousCount - defender.Count,
                RemainingCount: defender.Count,
                CurrentUnitHealth: defender.CurrentUnitHealth));
        }

        return result;
    }
}
