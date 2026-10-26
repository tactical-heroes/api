namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;

public sealed partial class MatchmakingReason : ValueObject
{
    public const int MaxLength = 256;

    private MatchmakingReason(string value)
    {
        Value = value;
    }

    public string Value
    {
        get;
    }

    public static Result<MatchmakingReason> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaxLength)
        {
            return Result.Failure<MatchmakingReason>(
                Error.Validation($"Reason must contain between 1 and {MaxLength} characters."));
        }
        return Result.Success(new MatchmakingReason(value.Trim()));
    }
}
