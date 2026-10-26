using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;

public sealed partial class MatchmakingTicketStatus : Enumeration<MatchmakingTicketStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchmakingTicketStatus Searching = new(
        id: 1, name: nameof(Searching), displayName: "Поиск матча");

    public static readonly MatchmakingTicketStatus Reserved = new(
        id: 2, name: nameof(Reserved), displayName: "Пара найдена");

    public static readonly MatchmakingTicketStatus Assigned = new(
        id: 3, name: nameof(Assigned), displayName: "Игра назначена");

    public static readonly MatchmakingTicketStatus Cancelled = new(
        id: 4, name: nameof(Cancelled), displayName: "Поиск отменён");

    public static readonly MatchmakingTicketStatus Expired = new(
        id: 5, name: nameof(Expired), displayName: "Время поиска истекло");

    private MatchmakingTicketStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<MatchmakingTicketStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchmakingTicketStatus>(Error.Validation("Unknown MatchmakingTicketStatus."));
    }
}
