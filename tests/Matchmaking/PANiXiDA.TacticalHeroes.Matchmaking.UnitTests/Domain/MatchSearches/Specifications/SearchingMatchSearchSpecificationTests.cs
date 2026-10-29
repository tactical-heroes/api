using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Specifications;

public sealed class SearchingMatchSearchSpecificationTests
{
    [Theory(DisplayName = "Specification should respect queue boundaries when current time changes")]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    public void IsSatisfiedBy_Should_RespectQueueBoundaries_When_CurrentTimeChanges(int seconds, bool expected)
    {
        var search = CreateSearch();
        var specification = new SearchingMatchSearchSpecification(At(seconds));

        var result = specification.IsSatisfiedBy(search);

        result.ShouldBe(expected);
    }

    [Theory(DisplayName = "Specification should reject inactive searches when status changes")]
    [InlineData("Cancelled")]
    [InlineData("Matched")]
    [InlineData("Closed")]
    [InlineData("Expired")]
    public void IsSatisfiedBy_Should_RejectInactiveSearches_When_StatusChanges(string status)
    {
        var search = CreateSearch();
        if (status == "Cancelled")
        {
            search.Cancel().IsSuccess.ShouldBeTrue();
        }
        else if (status == "Expired")
        {
            search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }
        else
        {
            search.Matched(At(1)).IsSuccess.ShouldBeTrue();
            if (status == "Closed")
            {
                search.Close().IsSuccess.ShouldBeTrue();
            }
        }

        var specification = new SearchingMatchSearchSpecification(At(1));

        var result = specification.IsSatisfiedBy(search);

        result.ShouldBeFalse();
    }
}
