using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchmakingTickets.ValueObjects;

public sealed class BuildSnapshotReferenceTests
{
    [Fact(DisplayName = "Matchmaking should validate identifier when created")]
    public void Create_Should_ValidateIdentifier_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = BuildSnapshotReference.Create(value);
        var empty = BuildSnapshotReference.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var reference = BuildSnapshotReference.Create(Guid.CreateVersion7()).Value;

        reference.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
