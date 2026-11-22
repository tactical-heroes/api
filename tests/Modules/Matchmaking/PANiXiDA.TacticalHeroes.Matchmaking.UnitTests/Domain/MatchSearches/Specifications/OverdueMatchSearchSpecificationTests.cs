using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Specifications;

public sealed class OverdueMatchSearchSpecificationTests
{
    [Theory(DisplayName = "Specification should select overdue searches when expiration boundary is reached")]
    [InlineData(299, false)]
    [InlineData(300, true)]
    [InlineData(301, true)]
    public void IsSatisfiedBy_Should_SelectOverdueSearches_When_ExpirationBoundaryIsReached(int seconds, bool expected)
    {
        var search = CreateSearch();
        var specification = new OverdueMatchSearchSpecification(At(seconds));

        var result = specification.IsSatisfiedBy(search);

        result.ShouldBe(expected);
    }

    [Theory(DisplayName = "Specification should preserve other states when search is no longer searching")]
    [InlineData("Matched")]
    [InlineData("Closed")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    public void IsSatisfiedBy_Should_PreserveOtherStates_When_SearchIsNoLongerSearching(string status)
    {
        var search = CreateSearch();
        if (status is "Matched" or "Closed")
        {
            search.Matched(At(1)).IsSuccess.ShouldBeTrue();
            if (status == "Closed")
            {
                search.Close().IsSuccess.ShouldBeTrue();
            }
        }
        else if (status == "Cancelled")
        {
            search.Cancel().IsSuccess.ShouldBeTrue();
        }
        else
        {
            search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }

        var result = new OverdueMatchSearchSpecification(At(301)).IsSatisfiedBy(search);

        result.ShouldBeFalse();
    }
}
