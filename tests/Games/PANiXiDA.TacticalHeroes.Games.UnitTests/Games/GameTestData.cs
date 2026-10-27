using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Games;

internal static class GameTestData
{
    internal static Game Create(GameMode? mode = null)
    {
        return Game.Create(
            mode ?? GameMode.RankedDuel,
            PlayerId.New(),
            BuildId.New(),
            Rating.Create(1000),
            PlayerId.New(),
            BuildId.New(),
            Rating.Create(1200)).Value;
    }

    internal static GameTimestamp At(int seconds)
    {
        return GameTimestamp.Create(new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }
}
