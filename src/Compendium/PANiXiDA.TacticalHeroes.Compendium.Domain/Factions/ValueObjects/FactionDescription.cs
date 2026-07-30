namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

public sealed class FactionDescription : ValueObject
{
    public const int MaxLength = 2000;

    private FactionDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FactionDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            return Result.Failure<FactionDescription>(
                error: Error.Validation(message: "Faction description cannot be empty.")
                    .WithField(field: nameof(FactionDescription)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new FactionDescription(value: normalizedValue))
            : Result.Failure<FactionDescription>(
                error: Error.Validation(
                    message: $"Faction description cannot be longer than {MaxLength} characters.")
                    .WithField(field: nameof(FactionDescription)));
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
