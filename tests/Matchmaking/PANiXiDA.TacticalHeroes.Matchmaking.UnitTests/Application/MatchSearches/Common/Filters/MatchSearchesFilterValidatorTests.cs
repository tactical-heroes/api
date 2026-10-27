using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Common.Filters;

public sealed class MatchSearchesFilterValidatorTests
{
    [Fact(DisplayName = "Filter should accept empty filter when no criteria are supplied")]
    public void Validate_Should_AcceptEmptyFilter_When_NoCriteriaAreSupplied()
    {
        var result = new MatchSearchesFilterValidator().Validate(new MatchSearchesFilter());

        result.IsValid.ShouldBeTrue();
    }
}
