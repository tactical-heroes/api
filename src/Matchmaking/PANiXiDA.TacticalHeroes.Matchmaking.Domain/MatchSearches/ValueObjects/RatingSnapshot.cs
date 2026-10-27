namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

public sealed partial class RatingSnapshot : ValueObject
{
    private RatingSnapshot(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static RatingSnapshot Create(int value)
    {
        return new RatingSnapshot(value);
    }
}
