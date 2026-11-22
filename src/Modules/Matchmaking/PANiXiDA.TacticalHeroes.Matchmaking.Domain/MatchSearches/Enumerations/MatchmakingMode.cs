using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

public sealed partial class MatchmakingMode : Enumeration<MatchmakingMode>
{
    public const int MaxLength = 64;

    public static readonly MatchmakingMode RankedDuel = new(
        id: 1, name: nameof(RankedDuel), displayName: "Рейтинговый 1×1");

    public static readonly MatchmakingMode CasualDuel = new(
        id: 2, name: nameof(CasualDuel), displayName: "Обычный 1×1");

    private MatchmakingMode(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static Result<MatchmakingMode> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), out var mode) && mode is not null
            ? Result.Success(mode)
            : Result.Failure<MatchmakingMode>(Error.Validation("Unknown MatchmakingMode."));
    }
}
