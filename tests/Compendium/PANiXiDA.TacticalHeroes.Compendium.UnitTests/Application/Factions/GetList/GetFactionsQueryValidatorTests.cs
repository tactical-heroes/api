using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetList;

public sealed class GetFactionsQueryValidatorTests
{
    [Fact(DisplayName = "Factions validator should reject invalid pagination")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetFactionsQueryValidator();

        var result = validator.Validate(
            new GetFactionsQuery(new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(
            error => error.PropertyName.EndsWith(
                nameof(PaginationParameters.PageNumber),
                StringComparison.Ordinal));
        result.Errors.ShouldContain(
            error => error.PropertyName.EndsWith(
                nameof(PaginationParameters.PageSize),
                StringComparison.Ordinal));
    }
}
