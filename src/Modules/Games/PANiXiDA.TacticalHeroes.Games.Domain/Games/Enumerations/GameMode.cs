using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;

public sealed partial class GameMode : Enumeration<GameMode>
{
    public const int MaxLength = 32;

    public static readonly GameMode RankedDuel = new(
        id: 1,
        name: nameof(RankedDuel),
        displayName: "Рейтинговый 1×1");

    public static readonly GameMode CasualDuel = new(
        id: 2,
        name: nameof(CasualDuel),
        displayName: "Обычный 1×1");

    private GameMode(int id, string name, string displayName) : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<GameMode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<GameMode>(
                Error.Validation("Game mode is required.")
                    .WithField(nameof(GameMode)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<GameMode>(
                Error.Validation($"Game mode '{normalizedValue}' is invalid.")
                    .WithField(nameof(GameMode)));
    }
}
