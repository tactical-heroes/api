using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.ValueObjects;

public sealed class MatchmakingReasonTests
{
    [Fact(DisplayName = "Closure reason should normalize and validate input when created")]
    public void Create_Should_NormalizeAndValidateInput_When_Created()
    {
        var normalized = MatchmakingReason.Create("  Network failure  ");

        normalized.Value.Value.ShouldBe("Network failure");
        MatchmakingReason.Create("").IsFailure.ShouldBeTrue();
        MatchmakingReason.Create(" ").IsFailure.ShouldBeTrue();
        MatchmakingReason.Create(new string('x', MatchmakingReason.MaxLength)).IsSuccess.ShouldBeTrue();
        MatchmakingReason.Create(new string('x', MatchmakingReason.MaxLength + 1)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Closure reason should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var value = MatchmakingReason.Create("Network failure").Value;

        value.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
