using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetList;

public sealed class GetUserListQueryValidatorTests
{
    [Fact(DisplayName = "User list validator should accept valid filters and pagination when query is valid")]
    public void Validate_Should_ReturnValidResult_When_QueryIsValid()
    {
        var validator = new GetUserListQueryValidator();

        var result = validator.Validate(
            new GetUserListQuery("hero@example.com", new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User list validator should reject invalid filters and pagination when query is invalid")]
    public void Validate_Should_ReturnErrors_When_QueryIsInvalid()
    {
        var validator = new GetUserListQueryValidator();

        var result = validator.Validate(
            new GetUserListQuery("invalid", new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUserListQuery.Email));
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }
}
