namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

public sealed partial class MatchmakingMode : ValueObject
{
    public const int MaxLength = 64;

    private MatchmakingMode(string value)
    {
        Value = value;
    }

    public string Value
    {
        get;
    }

    public static Result<MatchmakingMode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaxLength)
        {
            return Result.Failure<MatchmakingMode>(
                Error.Validation($"Mode must contain between 1 and {MaxLength} characters."));
        }
        return Result.Success(new MatchmakingMode(value.Trim().ToLowerInvariant()));
    }
}
