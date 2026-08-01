using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.GetList;

public sealed class GetUnitsQueryValidatorTests
{
    [Fact(DisplayName = "Units validator should reject invalid pagination when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetUnitsQueryValidator();

        var result = validator.Validate(
            new GetUnitsQuery(new PaginationParameters(0, 0)));

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
