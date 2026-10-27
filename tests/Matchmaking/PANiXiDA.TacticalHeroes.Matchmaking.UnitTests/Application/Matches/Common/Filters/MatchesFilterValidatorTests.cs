using PANiXiDA.TacticalHeroes.Matchmaking.Application.Matches.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.Matches.Common.Filters;

public sealed class MatchesFilterValidatorTests
{
    [Fact(DisplayName = "Filter should accept empty filter when no criteria are supplied")]
    public void Validate_Should_AcceptEmptyFilter_When_NoCriteriaAreSupplied()
    {
        var result = new MatchesFilterValidator().Validate(new MatchesFilter());

        result.IsValid.ShouldBeTrue();
    }
}
