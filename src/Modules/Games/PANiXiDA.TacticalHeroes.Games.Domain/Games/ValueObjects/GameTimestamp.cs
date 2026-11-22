namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;

public sealed partial class GameTimestamp : ValueObject
{
    private GameTimestamp(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; }

    public static GameTimestamp Create(DateTimeOffset value)
    {
        return new GameTimestamp(value.ToUniversalTime());
    }
}
