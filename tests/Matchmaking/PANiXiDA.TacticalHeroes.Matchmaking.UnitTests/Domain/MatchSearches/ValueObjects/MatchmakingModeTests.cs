using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.ValueObjects;

public sealed class MatchmakingModeTests
{
    [Fact(DisplayName = "Mode should normalize and validate input when created")]
    public void Create_Should_NormalizeAndValidateInput_When_Created()
    {
        var normalized = MatchmakingMode.Create("  Duel  ");

        normalized.Value.Value.ShouldBe("duel");
        MatchmakingMode.Create("").IsFailure.ShouldBeTrue();
        MatchmakingMode.Create(" ").IsFailure.ShouldBeTrue();
        MatchmakingMode.Create(new string('x', MatchmakingMode.MaxLength)).IsSuccess.ShouldBeTrue();
        MatchmakingMode.Create(new string('x', MatchmakingMode.MaxLength + 1)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Mode should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var value = MatchmakingMode.Create("duel").Value;

        value.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
