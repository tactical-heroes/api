using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetDetails;

public sealed class GetFactionDetailsQueryValidatorTests
{
    [Fact(DisplayName = "Faction details validator should reject an empty identifier")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new GetFactionDetailsQueryValidator();

        var result = validator.Validate(new GetFactionDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(GetFactionDetailsQuery.Id));
    }
}
