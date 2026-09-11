namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed class HeroDescription : ValueObject
{
    public const int MaxLength = 2000;

    private HeroDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<HeroDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<HeroDescription>(
                error: Error.Validation(message: "Hero description cannot be empty.")
                    .WithField(nameof(HeroDescription)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new HeroDescription(value: normalizedValue))
            : Result.Failure<HeroDescription>(
                error: Error.Validation(
                        message: $"Hero description cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(HeroDescription)));
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
