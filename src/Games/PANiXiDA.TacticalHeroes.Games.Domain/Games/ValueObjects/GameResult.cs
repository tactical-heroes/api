using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;

public sealed partial class GameResult : ValueObject
{
    private GameResult(bool isDraw, PlayerId? winnerPlayerId)
    {
        IsDraw = isDraw;
        WinnerPlayerId = winnerPlayerId;
    }

    public bool IsDraw { get; }
    public PlayerId? WinnerPlayerId { get; }

    public static GameResult Victory(PlayerId winnerPlayerId)
    {
        return new GameResult(isDraw: false, winnerPlayerId: winnerPlayerId);
    }

    public static GameResult Draw()
    {
        return new GameResult(isDraw: true, winnerPlayerId: null);
    }
}
