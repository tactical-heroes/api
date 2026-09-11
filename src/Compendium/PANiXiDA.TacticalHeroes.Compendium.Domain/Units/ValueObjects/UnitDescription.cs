namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed class UnitDescription : ValueObject
{
    public const int MaxLength = 2000;

    private UnitDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<UnitDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UnitDescription>(
                error: Error.Validation(message: "Unit description cannot be empty.")
                    .WithField(nameof(UnitDescription)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new UnitDescription(value: normalizedValue))
            : Result.Failure<UnitDescription>(
                error: Error.Validation(
                        message: $"Unit description cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(UnitDescription)));
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
