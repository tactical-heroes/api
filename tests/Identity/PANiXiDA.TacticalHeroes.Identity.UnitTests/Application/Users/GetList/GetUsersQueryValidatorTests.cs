using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetList;

public sealed class GetUsersQueryValidatorTests
{
    [Fact(DisplayName = "User list validator should return a valid result when email filter is partial")]
    public void Validate_Should_ReturnValidResult_When_EmailFilterIsPartial()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery("hero", new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User list validator should return errors when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery("invalid", new PaginationParameters(0, 0)));

        result.Errors.ShouldNotContain(error => error.PropertyName == nameof(GetUsersQuery.Email));
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }
}
