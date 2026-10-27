using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.ValueObjects;

public sealed class GameResultTests
{
    [Fact(DisplayName = "Game result should identify the winner when victory is created")]
    public void Victory_Should_IdentifyWinner_When_VictoryIsCreated()
    {
        var winner = PlayerId.New();

        var result = GameResult.Victory(winner);

        result.IsDraw.ShouldBeFalse();
        result.WinnerPlayerId.ShouldBe(winner);
        result.ShouldBe(GameResult.Victory(winner));
        result.ShouldNotBe(GameResult.Victory(PlayerId.New()));
        result.ShouldNotBe(GameResult.Draw());
    }

    [Fact(DisplayName = "Game result should have no winner when draw is created")]
    public void Draw_Should_HaveNoWinner_When_DrawIsCreated()
    {
        var result = GameResult.Draw();

        result.IsDraw.ShouldBeTrue();
        result.WinnerPlayerId.ShouldBeNull();
        result.ShouldBe(GameResult.Draw());
    }

    [Fact(DisplayName = "Game result should format its outcome when converted to string")]
    public void ToString_Should_FormatOutcome_When_ConvertedToString()
    {
        var result = GameResult.Draw();

        var formatted = result.ToString();

        formatted.ShouldBe("GameResult { IsDraw = True, WinnerPlayerId = null }");
    }
}
