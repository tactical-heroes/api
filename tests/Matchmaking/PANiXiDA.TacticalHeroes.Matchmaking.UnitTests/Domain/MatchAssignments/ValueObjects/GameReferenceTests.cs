using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.ValueObjects;

public sealed class GameReferenceTests
{
    [Fact(DisplayName = "Game reference should validate identifier when created")]
    public void Create_Should_ValidateIdentifier_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = GameReference.Create(value);
        var empty = GameReference.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Game reference should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var reference = GameReference.Create(Guid.CreateVersion7()).Value;

        reference.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
