using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

public sealed partial class MatchSearchStatus : Enumeration<MatchSearchStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchSearchStatus Searching = new(
        id: 1, name: nameof(Searching), displayName: "Поиск матча");

    public static readonly MatchSearchStatus Reserved = new(
        id: 2, name: nameof(Reserved), displayName: "Пара найдена");

    public static readonly MatchSearchStatus Assigned = new(
        id: 3, name: nameof(Assigned), displayName: "Игра назначена");

    public static readonly MatchSearchStatus Cancelled = new(
        id: 4, name: nameof(Cancelled), displayName: "Поиск отменён");

    public static readonly MatchSearchStatus Expired = new(
        id: 5, name: nameof(Expired), displayName: "Время поиска истекло");

    private MatchSearchStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<MatchSearchStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchSearchStatus>(Error.Validation("Unknown MatchSearchStatus."));
    }
}
