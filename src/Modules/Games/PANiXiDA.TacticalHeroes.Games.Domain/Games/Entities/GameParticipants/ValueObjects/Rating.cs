namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;

public sealed partial class Rating : ValueObject
{
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
