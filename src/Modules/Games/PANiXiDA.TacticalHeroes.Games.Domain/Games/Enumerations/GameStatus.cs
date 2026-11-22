using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;

public sealed partial class GameStatus : Enumeration<GameStatus>
{
    public const int MaxLength = 32;

    public static readonly GameStatus WaitingForPlayers = new(
        id: 1,
        name: nameof(WaitingForPlayers),
        displayName: "Ожидание игроков");

    public static readonly GameStatus InProgress = new(
        id: 2,
        name: nameof(InProgress),
        displayName: "Игра идёт");

    public static readonly GameStatus Finished = new(
        id: 3,
        name: nameof(Finished),
        displayName: "Игра завершена");

    public static readonly GameStatus Cancelled = new(
        id: 4,
        name: nameof(Cancelled),
        displayName: "Игра отменена");

    private GameStatus(int id, string name, string displayName) : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<GameStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<GameStatus>(
                Error.Validation("Game status is required.")
                    .WithField(nameof(GameStatus)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<GameStatus>(
                Error.Validation($"Game status '{normalizedValue}' is invalid.")
                    .WithField(nameof(GameStatus)));
    }
}
