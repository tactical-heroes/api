using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchmakingTickets.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchmakingTickets.Common.Filters;

public sealed class MatchmakingTicketsFilterValidatorTests
{
    [Fact(DisplayName = "Matchmaking should accept empty filter when no criteria are supplied")]
    public void Validate_Should_AcceptEmptyFilter_When_NoCriteriaAreSupplied()
    {
        var result = new MatchmakingTicketsFilterValidator().Validate(new MatchmakingTicketsFilter());

        result.IsValid.ShouldBeTrue();
    }
}
