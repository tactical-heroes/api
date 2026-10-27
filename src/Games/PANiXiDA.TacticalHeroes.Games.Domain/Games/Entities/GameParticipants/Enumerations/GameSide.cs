using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.Enumerations;

public sealed partial class GameSide : Enumeration<GameSide>
{
    public const int MaxLength = 32;

    public static readonly GameSide First = new(
        id: 1,
        name: nameof(First),
        displayName: "Первая сторона");

    public static readonly GameSide Second = new(
        id: 2,
        name: nameof(Second),
        displayName: "Вторая сторона");

    private GameSide(int id, string name, string displayName) : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<GameSide> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<GameSide>(
                Error.Validation("Game side is required.")
                    .WithField(nameof(GameSide)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<GameSide>(
                Error.Validation($"Game side '{normalizedValue}' is invalid.")
                    .WithField(nameof(GameSide)));
    }
}
