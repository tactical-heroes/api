namespace PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;

public sealed partial class Rating : ValueObject
{
    public const int InitialValue = 1000;

    private Rating(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Rating Create(int value)
    {
        return new Rating(value: value);
    }
}
