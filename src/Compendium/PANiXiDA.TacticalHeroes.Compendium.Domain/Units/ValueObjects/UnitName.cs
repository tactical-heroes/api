namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitName : ValueObject
{
    public const int MaxLength = 128;

    private UnitName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<UnitName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UnitName>(
                error: Error.Validation(message: "Unit name cannot be empty.")
                    .WithField(nameof(UnitName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new UnitName(value: normalizedValue))
            : Result.Failure<UnitName>(
                error: Error.Validation(
                        message: $"Unit name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(UnitName)));
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
