namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitLuck : ValueObject
{
    public const int Minimum = 0;
    public const int Maximum = 5;

    private UnitLuck(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<UnitLuck> Create(int value)
    {
        return value is >= Minimum and <= Maximum
            ? Result.Success(value: new UnitLuck(value: value))
            : Result.Failure<UnitLuck>(
                error: Error.Validation(
                        message: $"Unit luck must be between {Minimum} and {Maximum}.")
                    .WithField(nameof(UnitLuck)));
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
