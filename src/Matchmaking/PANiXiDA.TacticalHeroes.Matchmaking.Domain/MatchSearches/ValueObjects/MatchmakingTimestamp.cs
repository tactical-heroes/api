namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

public sealed partial class MatchmakingTimestamp : ValueObject
{
    private MatchmakingTimestamp(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; }

    public static MatchmakingTimestamp Create(DateTimeOffset value)
    {
        return new MatchmakingTimestamp(value.ToUniversalTime());
    }
}
