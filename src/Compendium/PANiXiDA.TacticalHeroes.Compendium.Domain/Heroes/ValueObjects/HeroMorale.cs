namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed class HeroMorale : ValueObject
{
    public const int Minimum = 0;
    public const int Maximum = 5;

    private HeroMorale(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<HeroMorale> Create(int value)
    {
        return value is >= Minimum and <= Maximum
            ? Result.Success(value: new HeroMorale(value: value))
            : Result.Failure<HeroMorale>(
                error: Error.Validation(
                        message: $"Hero morale must be between {Minimum} and {Maximum}.")
                    .WithField(nameof(HeroMorale)));
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
