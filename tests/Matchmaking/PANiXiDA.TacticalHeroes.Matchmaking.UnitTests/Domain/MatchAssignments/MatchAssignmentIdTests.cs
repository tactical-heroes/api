using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments;

public sealed class MatchAssignmentIdTests
{
    [Fact(DisplayName = "Matchmaking should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = MatchAssignmentId.Create(value);
        var empty = MatchAssignmentId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = MatchAssignmentId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(MatchAssignmentId.New());
    }

    [Fact(DisplayName = "Matchmaking should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = MatchAssignmentId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
