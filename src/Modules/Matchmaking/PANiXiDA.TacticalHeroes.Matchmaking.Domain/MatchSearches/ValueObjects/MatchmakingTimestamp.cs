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

    public static bool operator <(MatchmakingTimestamp left, MatchmakingTimestamp right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(MatchmakingTimestamp left, MatchmakingTimestamp right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <=(MatchmakingTimestamp left, MatchmakingTimestamp right)
    {
        return left.Value <= right.Value;
    }

    public static bool operator >=(MatchmakingTimestamp left, MatchmakingTimestamp right)
    {
        return left.Value >= right.Value;
    }
}
