using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetList;

public sealed class GetUsersQueryValidatorTests
{
    [Fact(DisplayName = "User list validator should accept valid filters and pagination")]
    public void Validate_Should_ReturnValidResult_When_QueryIsValid()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery("hero@example.com", new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User list validator should reject invalid filters and pagination")]
    public void Validate_Should_ReturnErrors_When_QueryIsInvalid()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery("invalid", new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUsersQuery.Email));
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }
}
