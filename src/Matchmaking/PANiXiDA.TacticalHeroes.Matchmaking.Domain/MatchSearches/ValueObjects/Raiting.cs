namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

public sealed partial class Raiting : ValueObject
{
    private Raiting(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Raiting Create(int value)
    {
        return new Raiting(value);
    }
}
