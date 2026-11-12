using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

public sealed class Battle : AggregateRoot<BattleId>
{
    private readonly List<Unit> _units = [];
    private readonly List<Hero> _heroes = [];

    private Battle(BattleId id) : base(id)
    {
    }

    public GameId? GameId { get; private set; }
    public PlayerId? FirstPlayerId { get; private set; }
    public PlayerId? SecondPlayerId { get; private set; }
    public BattleStatus Status { get; private set; } = BattleStatus.InProgress;
    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();
    public IReadOnlyCollection<Hero> Heroes => _heroes.AsReadOnly();

    public static Battle Create()
    {
        return new Battle(BattleId.New());
    }

    public static Result<Battle> Create(GameId gameId, PlayerId firstPlayerId, PlayerId secondPlayerId)
    {
        if (gameId.Value == Guid.Empty
            || firstPlayerId.Value == Guid.Empty
            || secondPlayerId.Value == Guid.Empty
            || firstPlayerId == secondPlayerId)
        {
            return Result.Failure<Battle>(Error.Validation("A game battle requires two different players."));
        }

        var battle = new Battle(BattleId.New())
        {
            GameId = gameId,
            Status = BattleStatus.Preparing,
            FirstPlayerId = firstPlayerId,
            SecondPlayerId = secondPlayerId
        };
        battle.AddDomainEvent(new BattleCreated(GameId: gameId.Value, BattleId: battle.Id.Value));

        return Result.Success(battle);
    }

    public Result Start()
    {
        Status = BattleStatus.InProgress;

        return Result.Success();
    }

    public Result AddHeroes(IEnumerable<Hero> heroes)
    {
        var selected = heroes.ToArray();
        if (Status != BattleStatus.Preparing
            || selected.Any(hero => hero is null
                || hero.BattleId != Id
                || (hero.OwnerPlayerId != FirstPlayerId
                    && hero.OwnerPlayerId != SecondPlayerId))
            || selected.Select(hero => hero.Id).Distinct().Count() != selected.Length
            || selected.Any(hero => _heroes.Any(existing => existing.Id == hero.Id)))
        {
            return Result.Failure(Error.Validation("Heroes must belong to a participant of this preparing battle."));
        }

        _heroes.AddRange(selected);

        return Result.Success();
    }

    public Result AddUnits(IEnumerable<Unit> units)
    {
        var participants = units.ToArray();
        if ((GameId is not null
            && Status != BattleStatus.Preparing)
            || participants.Any(unit => unit is null
                || unit.BattleId != Id
                || (GameId is not null
                    && unit.OwnerPlayerId != FirstPlayerId
                    && unit.OwnerPlayerId != SecondPlayerId))
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
        if (Status != BattleStatus.InProgress)
        {
            return Result.Failure(Error.Conflict("Only a started battle can process attacks."));
        }

        if (attackerId == defenderId)
        {
            return Result.Failure(Error.Validation("A unit cannot attack itself."));
        }

        var attackerResult = GetUnit(attackerId);
        var defenderResult = GetUnit(defenderId);
        var validationResult = Result.Combine(
            attackerResult,
            defenderResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var attacker = attackerResult.Value;
        var defender = defenderResult.Value;
        var damage = attacker.CalculateDamage(defender.Stats);
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
                AttackerId: attacker.Id.Value,
                DefenderId: defender.Id.Value,
                CalculatedDamage: damage.Value.Value,
                AppliedDamage: appliedDamage,
                KilledCount: previousCount - defender.Count,
                RemainingCount: defender.Count,
                CurrentUnitHealth: defender.CurrentUnitHealth));
        }

        return result;
    }

    private Result<Unit> GetUnit(UnitId unitId)
    {
        var unit = _units.SingleOrDefault(unit => unit.Id == unitId);
        if (unit is null)
        {
            return Result.Failure<Unit>(Error.Validation($"Unit '{unitId}' was not found in this battle."));
        }

        if (!unit.IsAlive)
        {
            return Result.Failure<Unit>(Error.Validation($"Unit '{unitId}' is destroyed."));
        }

        return Result.Success(unit);
    }
}
