using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.ValueObjects;

public sealed class RatingTests
{
    [Theory(DisplayName = "Rating should preserve rating when created")]
    [InlineData(-50)]
    [InlineData(0)]
    [InlineData(1500)]
    public void Create_Should_PreserveRating_When_Created(int rating)
    {
        var snapshot = Rating.Create(rating);

        snapshot.Value.ShouldBe(rating);
    }

    [Fact(DisplayName = "Rating should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var snapshot = Rating.Create(1500);

        snapshot.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
