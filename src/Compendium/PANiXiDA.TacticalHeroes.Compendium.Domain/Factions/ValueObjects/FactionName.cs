namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

public sealed class FactionName : ValueObject
{
    public const int MaxLength = 128;

    private FactionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FactionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FactionName>(
                error: Error.Validation(message: "Faction name cannot be empty.")
                    .WithField(nameof(FactionName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new FactionName(value: normalizedValue))
            : Result.Failure<FactionName>(
                error: Error.Validation(
                        message: $"Faction name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(FactionName)));
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
