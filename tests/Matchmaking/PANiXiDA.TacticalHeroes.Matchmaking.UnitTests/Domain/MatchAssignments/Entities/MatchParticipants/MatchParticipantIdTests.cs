using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.Entities.MatchParticipants;

public sealed class MatchParticipantIdTests
{
    [Fact(DisplayName = "Participant id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = MatchParticipantId.Create(value);
        var empty = MatchParticipantId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Participant id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = MatchParticipantId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(MatchParticipantId.New());
    }

    [Fact(DisplayName = "Participant id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = MatchParticipantId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
