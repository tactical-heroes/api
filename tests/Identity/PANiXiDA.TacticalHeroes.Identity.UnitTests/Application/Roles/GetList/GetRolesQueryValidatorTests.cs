using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetList;

public sealed class GetRolesQueryValidatorTests
{
    [Fact(DisplayName = "Role list validator should accept valid pagination")]
    public void Validate_Should_ReturnValidResult_When_PaginationIsValid()
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(new GetRolesQuery(new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Role list validator should reject invalid pagination")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(new GetRolesQuery(new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }
}
