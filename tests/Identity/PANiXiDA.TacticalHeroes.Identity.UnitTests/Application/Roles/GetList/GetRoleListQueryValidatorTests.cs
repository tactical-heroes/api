using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetList;

public sealed class GetRoleListQueryValidatorTests
{
    [Fact(DisplayName = "Role list validator should accept valid pagination when pagination is valid")]
    public void Validate_Should_ReturnValidResult_When_PaginationIsValid()
    {
        var validator = new GetRoleListQueryValidator();

        var result = validator.Validate(new GetRoleListQuery(new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Role list validator should reject invalid pagination when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetRoleListQueryValidator();

        var result = validator.Validate(new GetRoleListQuery(new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }
}
