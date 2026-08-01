namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitMorale : ValueObject
{
    public const int Minimum = 0;
    public const int Maximum = 5;

    private UnitMorale(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<UnitMorale> Create(int value)
    {
        return value is >= Minimum and <= Maximum
            ? Result.Success(value: new UnitMorale(value: value))
            : Result.Failure<UnitMorale>(
                error: Error.Validation(
                        message: $"Unit morale must be between {Minimum} and {Maximum}.")
                    .WithField(nameof(UnitMorale)));
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
