using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.GetList;

public sealed class GetHeroesQueryValidatorTests
{
    [Fact(DisplayName = "Heroes validator should reject invalid pagination when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetHeroesQueryValidator();

        var result = validator.Validate(
            new GetHeroesQuery(new PaginationParameters(0, 0)));

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
