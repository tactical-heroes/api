using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;

public sealed partial class MatchStatus : Enumeration<MatchStatus>
{
    public const int MaxLength = 32;

    public static readonly MatchStatus CreatingGame = new(
        id: 1, name: nameof(CreatingGame), displayName: "Создание игры");

    public static readonly MatchStatus Ready = new(id: 2, name: nameof(Ready), displayName: "Игра создана");

    public static readonly MatchStatus Expired = new(
        id: 3, name: nameof(Expired), displayName: "Время ожидания истекло");

    public static readonly MatchStatus Failed = new(
        id: 4, name: nameof(Failed), displayName: "Ошибка создания игры");

    private MatchStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<MatchStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<MatchStatus>(Error.Validation("Unknown MatchStatus."));
    }
}
