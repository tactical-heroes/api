using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;

public sealed partial class MatchAssignmentStatus : Enumeration<MatchAssignmentStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchAssignmentStatus CreatingGame = new(
        id: 1, name: nameof(CreatingGame), displayName: "Создание игры");

    public static readonly MatchAssignmentStatus Ready = new(id: 2, name: nameof(Ready), displayName: "Игра создана");

    public static readonly MatchAssignmentStatus Expired = new(
        id: 3, name: nameof(Expired), displayName: "Время ожидания истекло");

    public static readonly MatchAssignmentStatus Failed = new(
        id: 4, name: nameof(Failed), displayName: "Ошибка создания игры");

    private MatchAssignmentStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<MatchAssignmentStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchAssignmentStatus>(Error.Validation("Unknown MatchAssignmentStatus."));
    }
}
