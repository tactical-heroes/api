using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Entities.GameParticipants.ValueObjects;

public sealed class RatingTests
{
    [Theory(DisplayName = "Rating should preserve its value when any integer is supplied")]
    [InlineData(int.MinValue)]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void Create_Should_PreserveValue_When_AnyIntegerIsSupplied(int value)
    {
        var rating = Rating.Create(value);

        rating.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Rating should compare by value when values are equal")]
    public void Equals_Should_CompareByValue_When_ValuesAreEqual()
    {
        var rating = Rating.Create(1200);
        var sameRating = Rating.Create(1200);
        var otherRating = Rating.Create(1300);

        rating.Equals(sameRating).ShouldBeTrue();
        rating.Equals(otherRating).ShouldBeFalse();
        rating.GetHashCode().ShouldBe(sameRating.GetHashCode());
    }

    [Fact(DisplayName = "Rating should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var rating = Rating.Create(1000);

        var result = rating.ToString();

        result.ShouldBe("Rating { Value = 1000 }");
    }
}
