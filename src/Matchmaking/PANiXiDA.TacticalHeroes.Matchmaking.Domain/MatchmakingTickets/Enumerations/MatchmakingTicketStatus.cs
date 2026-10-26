using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;

public sealed partial class MatchmakingTicketStatus : Enumeration<MatchmakingTicketStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchmakingTicketStatus Searching = new(1, nameof(Searching));

    public static readonly MatchmakingTicketStatus Reserved = new(2, nameof(Reserved));

    public static readonly MatchmakingTicketStatus Assigned = new(3, nameof(Assigned));

    public static readonly MatchmakingTicketStatus Cancelled = new(4, nameof(Cancelled));

    public static readonly MatchmakingTicketStatus Expired = new(5, nameof(Expired));

    private MatchmakingTicketStatus(int id, string name) : base(id, name)
    {
    }

    public static Result<MatchmakingTicketStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchmakingTicketStatus>(Error.Validation("Unknown MatchmakingTicketStatus."));
    }
}
