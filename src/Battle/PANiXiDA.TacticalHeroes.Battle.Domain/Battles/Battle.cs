using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

public sealed class Battle : AggregateRoot<BattleId>
{
    private readonly List<Unit> _units = [];

    private Battle(BattleId id) : base(id)
    {
    }

    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();

    public static Result<Battle> Create(IEnumerable<Unit> units)
    {
        return Create(BattleId.New(), units);
    }

    public static Result<Battle> Create(BattleId id, IEnumerable<Unit> units)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<Battle>(Error.Validation("Battle id cannot be empty."));
        }

        var participants = units.ToArray();
        if (participants.Length < 2
            || participants.Any(unit => unit is null || unit.BattleId.HasValue)
            || participants.Select(unit => unit.Id).Distinct().Count() != participants.Length)
        {
            return Result.Failure<Battle>(Error.Validation(
                "A battle requires at least two distinct, unassigned units."));
        }

        var battle = new Battle(id);
        foreach (var unit in participants)
        {
            unit.AssignToBattle(battle.Id);
            battle._units.Add(unit);
        }

        return Result.Success(battle);
    }

    public Result<DamageResult> Attack(UnitId attackerId, UnitId defenderId, DamageRoll roll)
    {
        if (attackerId == defenderId)
        {
            return Result.Failure<DamageResult>(Error.Validation("A unit cannot attack itself."));
        }

        var attacker = _units.SingleOrDefault(unit => unit.Id == attackerId);
        var defender = _units.SingleOrDefault(unit => unit.Id == defenderId);
        if (attacker is null || defender is null || !attacker.IsAlive || !defender.IsAlive)
        {
            return Result.Failure<DamageResult>(Error.Validation(
                "Both participants must be living units in this battle."));
        }

        var damage = DamageCalculator.Calculate(
            attacker.Stats, attacker.StackState, attacker.DamageRange, defender.Stats, roll);
        if (damage.IsFailure)
        {
            return Result.Failure<DamageResult>(damage.Errors);
        }

        var result = defender.TakeDamage(damage.Value);
        if (result.IsSuccess && result.Value.AppliedDamage.Value > 0)
        {
            AddDomainEvent(new UnitDamaged(Id, attackerId, defenderId, result.Value));
        }

        return result;
    }
}
