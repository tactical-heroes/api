namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitRangedAttack : ValueObject
{
    private UnitRangedAttack(
        int? shots,
        int? rangedAttackRange)
    {
        Shots = shots;
        RangedAttackRange = rangedAttackRange;
    }

    public int? Shots { get; }
    public int? RangedAttackRange { get; }

    public static Result<UnitRangedAttack> Create(
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
        var rangedAttackRangeResult = !rangedAttackRange.HasValue || rangedAttackRange.Value > 0
            ? Result.Success()
            : Result.Failure(
                error: Error.Validation(
                        message: "Unit ranged attack range must be greater than zero when provided.")
                    .WithField(nameof(RangedAttackRange)));
        var validationResult = Result.Combine(pairResult, shotsResult, rangedAttackRangeResult);

        return validationResult.IsFailure
            ? Result.Failure<UnitRangedAttack>(errors: validationResult.Errors)
            : Result.Success(value: new UnitRangedAttack(shots, rangedAttackRange));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Shots;
        yield return RangedAttackRange;
    }
}
