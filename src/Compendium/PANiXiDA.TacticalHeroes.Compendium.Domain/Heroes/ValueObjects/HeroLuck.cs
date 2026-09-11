namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed class HeroLuck : ValueObject
{
    public const int Minimum = 0;
    public const int Maximum = 5;

    private HeroLuck(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<HeroLuck> Create(int value)
    {
        return value is >= Minimum and <= Maximum
            ? Result.Success(value: new HeroLuck(value: value))
            : Result.Failure<HeroLuck>(
                error: Error.Validation(
                        message: $"Hero luck must be between {Minimum} and {Maximum}.")
                    .WithField(nameof(HeroLuck)));
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
