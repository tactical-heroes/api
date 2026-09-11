namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed class HeroCombatStats : ValueObject
{
    private HeroCombatStats(
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative)
    {
        Attack = attack;
        Defense = defense;
        MinimumDamage = minimumDamage;
        MaximumDamage = maximumDamage;
        Initiative = initiative;
    }

    public int Attack { get; }
    public int Defense { get; }
    public int MinimumDamage { get; }
    public int MaximumDamage { get; }
    public double Initiative { get; }

    public static Result<HeroCombatStats> Create(
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative)
    {
        var attackResult = ValidateNonNegative(
            value: attack,
            field: nameof(Attack),
            message: "Hero attack cannot be negative.");
        var defenseResult = ValidateNonNegative(
            value: defense,
            field: nameof(Defense),
            message: "Hero defense cannot be negative.");
        var minimumDamageResult = ValidateNonNegative(
            value: minimumDamage,
            field: nameof(MinimumDamage),
            message: "Hero minimum damage cannot be negative.");
        var maximumDamageResult = ValidateNonNegative(
            value: maximumDamage,
            field: nameof(MaximumDamage),
            message: "Hero maximum damage cannot be negative.");
        var damageRangeResult = maximumDamage >= minimumDamage
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Hero maximum damage cannot be less than minimum damage.")
                    .WithField(nameof(MaximumDamage)));
        var initiativeResult = double.IsFinite(initiative) && initiative >= 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Hero initiative must be a finite non-negative number.")
                    .WithField(nameof(Initiative)));
        var validationResult = Result.Combine(
            attackResult,
            defenseResult,
            minimumDamageResult,
            maximumDamageResult,
            damageRangeResult,
            initiativeResult);

        return validationResult.IsFailure
            ? Result.Failure<HeroCombatStats>(errors: validationResult.Errors)
            : Result.Success(
                value: new HeroCombatStats(
                    attack: attack,
                    defense: defense,
                    minimumDamage: minimumDamage,
                    maximumDamage: maximumDamage,
                    initiative: initiative));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Attack;
        yield return Defense;
        yield return MinimumDamage;
        yield return MaximumDamage;
        yield return Initiative;
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
}
