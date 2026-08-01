namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed class HeroName : ValueObject
{
    public const int MaxLength = 128;

    private HeroName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<HeroName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<HeroName>(
                error: Error.Validation(message: "Hero name cannot be empty.")
                    .WithField(nameof(HeroName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new HeroName(value: normalizedValue))
            : Result.Failure<HeroName>(
                error: Error.Validation(
                        message: $"Hero name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(HeroName)));
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
