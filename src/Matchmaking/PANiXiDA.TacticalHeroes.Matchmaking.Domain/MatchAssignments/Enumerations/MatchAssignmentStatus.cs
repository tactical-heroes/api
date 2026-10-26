using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;

public sealed partial class MatchAssignmentStatus : Enumeration<MatchAssignmentStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchAssignmentStatus AwaitingAcceptance = new(1, nameof(AwaitingAcceptance));

    public static readonly MatchAssignmentStatus CreatingGame = new(2, nameof(CreatingGame));

    public static readonly MatchAssignmentStatus Ready = new(3, nameof(Ready));

    public static readonly MatchAssignmentStatus Cancelled = new(4, nameof(Cancelled));

    public static readonly MatchAssignmentStatus Expired = new(5, nameof(Expired));

    public static readonly MatchAssignmentStatus Failed = new(6, nameof(Failed));

    private MatchAssignmentStatus(int id, string name) : base(id, name)
    {
    }

    public static Result<MatchAssignmentStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchAssignmentStatus>(Error.Validation("Unknown MatchAssignmentStatus."));
    }
}
