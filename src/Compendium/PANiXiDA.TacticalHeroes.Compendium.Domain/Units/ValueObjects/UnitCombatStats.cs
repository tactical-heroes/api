namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitCombatStats : ValueObject
{
    private UnitCombatStats()
    {
    }

    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Health { get; private set; }
    public int MinimumDamage { get; private set; }
    public int MaximumDamage { get; private set; }
    public double Initiative { get; private set; }
    public int Speed { get; private set; }
    public int? Shots { get; private set; }
    public int? RangedAttackRange { get; private set; }

    public static Result<UnitCombatStats> Create(UnitCombatStatsInput input)
    {
        var attackResult = ValidateNonNegative(
            value: input.Attack,
            field: nameof(Attack),
            message: "Unit attack cannot be negative.");
        var defenseResult = ValidateNonNegative(
            value: input.Defense,
            field: nameof(Defense),
            message: "Unit defense cannot be negative.");
        var healthResult = ValidatePositive(
            value: input.Health,
            field: nameof(Health),
            message: "Unit health must be greater than zero.");
        var minimumDamageResult = ValidateNonNegative(
            value: input.MinimumDamage,
            field: nameof(MinimumDamage),
            message: "Unit minimum damage cannot be negative.");
        var maximumDamageResult = ValidateNonNegative(
            value: input.MaximumDamage,
            field: nameof(MaximumDamage),
            message: "Unit maximum damage cannot be negative.");
        var damageRangeResult = input.MaximumDamage >= input.MinimumDamage
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit maximum damage cannot be less than minimum damage.")
                    .WithField(nameof(MaximumDamage)));
        var initiativeResult = ValidateNonNegativeFinite(
            value: input.Initiative,
            field: nameof(Initiative),
            message: "Unit initiative must be a finite non-negative number.");
        var speedResult = ValidateNonNegative(
            value: input.Speed,
            field: nameof(Speed),
            message: "Unit speed cannot be negative.");
        var rangedAttackResult = ValidateRangedAttack(
            shots: input.Shots,
            rangedAttackRange: input.RangedAttackRange);
        var validationResult = Result.Combine(
            attackResult,
            defenseResult,
            healthResult,
            minimumDamageResult,
            maximumDamageResult,
            damageRangeResult,
            initiativeResult,
            speedResult,
            rangedAttackResult);

        return validationResult.IsFailure
            ? Result.Failure<UnitCombatStats>(errors: validationResult.Errors)
            : Result.Success(
                value: new UnitCombatStats
                {
                    Attack = input.Attack,
                    Defense = input.Defense,
                    Health = input.Health,
                    MinimumDamage = input.MinimumDamage,
                    MaximumDamage = input.MaximumDamage,
                    Initiative = input.Initiative,
                    Speed = input.Speed,
                    Shots = input.Shots,
                    RangedAttackRange = input.RangedAttackRange
                });
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Attack;
        yield return Defense;
        yield return Health;
        yield return MinimumDamage;
        yield return MaximumDamage;
        yield return Initiative;
        yield return Speed;
        yield return Shots;
        yield return RangedAttackRange;
    }

    private static Result ValidateNonNegative(
        int value,
        string field,
        string message)
    {
        return value >= 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(message: message)
                    .WithField(field));
    }

    private static Result ValidatePositive(
        int value,
        string field,
        string message)
    {
        return value > 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(message: message)
                    .WithField(field));
    }

    private static Result ValidateNonNegativeFinite(
        double value,
        string field,
        string message)
    {
        return double.IsFinite(value) && value >= 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(message: message)
                    .WithField(field));
    }

    private static Result ValidateRangedAttack(
        int? shots,
        int? rangedAttackRange)
    {
        var pairResult = shots.HasValue == rangedAttackRange.HasValue
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit shots and ranged attack range must both be provided or both be omitted.")
                    .WithField(nameof(RangedAttackRange)));
        var shotsResult = !shots.HasValue || shots.Value > 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit shots must be greater than zero when provided.")
                    .WithField(nameof(Shots)));
        var rangedAttackRangeResult = !rangedAttackRange.HasValue ||
                                      rangedAttackRange.Value > 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit ranged attack range must be greater than zero when provided.")
                    .WithField(nameof(RangedAttackRange)));

        return Result.Combine(
            pairResult,
            shotsResult,
            rangedAttackRangeResult);
    }
}
