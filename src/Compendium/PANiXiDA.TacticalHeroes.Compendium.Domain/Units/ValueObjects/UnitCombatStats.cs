namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitCombatStats : ValueObject
{
    private UnitCombatStats(
        int attack,
        int defense,
        int health,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int speed)
    {
        Attack = attack;
        Defense = defense;
        Health = health;
        MinimumDamage = minimumDamage;
        MaximumDamage = maximumDamage;
        Initiative = initiative;
        Speed = speed;
    }

    public int Attack { get; }

    public int Defense { get; }

    public int Health { get; }

    public int MinimumDamage { get; }

    public int MaximumDamage { get; }

    public double Initiative { get; }

    public int Speed { get; }

    public static Result<UnitCombatStats> Create(
        int attack,
        int defense,
        int health,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int speed)
    {
        var attackResult = ValidateNonNegative(
            value: attack,
            field: nameof(Attack),
            message: "Unit attack cannot be negative.");
        var defenseResult = ValidateNonNegative(
            value: defense,
            field: nameof(Defense),
            message: "Unit defense cannot be negative.");
        var healthResult = ValidatePositive(
            value: health,
            field: nameof(Health),
            message: "Unit health must be greater than zero.");
        var minimumDamageResult = ValidateNonNegative(
            value: minimumDamage,
            field: nameof(MinimumDamage),
            message: "Unit minimum damage cannot be negative.");
        var maximumDamageResult = ValidateNonNegative(
            value: maximumDamage,
            field: nameof(MaximumDamage),
            message: "Unit maximum damage cannot be negative.");
        var damageRangeResult = maximumDamage >= minimumDamage
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit maximum damage cannot be less than minimum damage.")
                    .WithField(nameof(MaximumDamage)));
        var initiativeResult = ValidateNonNegativeFinite(
            value: initiative,
            field: nameof(Initiative),
            message: "Unit initiative must be a finite non-negative number.");
        var speedResult = ValidateNonNegative(
            value: speed,
            field: nameof(Speed),
            message: "Unit speed cannot be negative.");
        var validationResult = Result.Combine(
            attackResult,
            defenseResult,
            healthResult,
            minimumDamageResult,
            maximumDamageResult,
            damageRangeResult,
            initiativeResult,
            speedResult);

        return validationResult.IsFailure
            ? Result.Failure<UnitCombatStats>(errors: validationResult.Errors)
            : Result.Success(
                value: new UnitCombatStats(
                    attack: attack,
                    defense: defense,
                    health: health,
                    minimumDamage: minimumDamage,
                    maximumDamage: maximumDamage,
                    initiative: initiative,
                    speed: speed));
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
}
