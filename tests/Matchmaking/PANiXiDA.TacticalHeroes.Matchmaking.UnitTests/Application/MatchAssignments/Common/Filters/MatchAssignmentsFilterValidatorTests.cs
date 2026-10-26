using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchAssignments.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchAssignments.Common.Filters;

public sealed class MatchAssignmentsFilterValidatorTests
{
    [Fact(DisplayName = "Filter should accept empty filter when no criteria are supplied")]
    public void Validate_Should_AcceptEmptyFilter_When_NoCriteriaAreSupplied()
    {
        var result = new MatchAssignmentsFilterValidator().Validate(new MatchAssignmentsFilter());

        result.IsValid.ShouldBeTrue();
    }
}
