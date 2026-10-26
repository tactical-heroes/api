using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchmakingTickets.ValueObjects;

public sealed class RatingSnapshotTests
{
    [Theory(DisplayName = "Rating snapshot should preserve rating when created")]
    [InlineData(-50)]
    [InlineData(0)]
    [InlineData(1500)]
    public void Create_Should_PreserveRating_When_Created(int rating)
    {
        var snapshot = RatingSnapshot.Create(rating);

        snapshot.Value.ShouldBe(rating);
    }

    [Fact(DisplayName = "Rating snapshot should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var snapshot = RatingSnapshot.Create(1500);

        snapshot.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
