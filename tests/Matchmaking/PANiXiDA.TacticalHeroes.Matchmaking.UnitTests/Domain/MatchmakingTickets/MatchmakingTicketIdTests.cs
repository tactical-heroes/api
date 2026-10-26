using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchmakingTickets;

public sealed class MatchmakingTicketIdTests
{
    [Fact(DisplayName = "Matchmaking should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = MatchmakingTicketId.Create(value);
        var empty = MatchmakingTicketId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = MatchmakingTicketId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(MatchmakingTicketId.New());
    }

    [Fact(DisplayName = "Matchmaking should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = MatchmakingTicketId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
