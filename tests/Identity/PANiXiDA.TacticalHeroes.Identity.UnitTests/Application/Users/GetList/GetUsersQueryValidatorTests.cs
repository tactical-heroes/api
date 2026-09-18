using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetList;

public sealed class GetUsersQueryValidatorTests
{
    [Fact(DisplayName = "User list validator should return a valid result when email filter is partial")]
    public void Validate_Should_ReturnValidResult_When_EmailFilterIsPartial()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery(new UsersFilter("hero"), new PaginationParameters(1, 20), SortingParameters.None));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User list validator should return errors when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery(new UsersFilter("invalid"), new PaginationParameters(0, 0), SortingParameters.None));

        result.Errors.ShouldNotContain(error => error.PropertyName == "Filter.Email");
        result.Errors.ShouldContain(error => error.PropertyName == "PaginationParameters.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "PaginationParameters.PageSize");
    }

    [Theory(DisplayName = "Validate should apply shared pagination rules when page size or offset exceeds bounds")]
    [InlineData(1, 201, "PaginationParameters.PageSize")]
    [InlineData(int.MaxValue, 2, "PaginationParameters.PageNumber")]
    public void Validate_Should_ApplySharedPaginationRules_When_PageSizeOrOffsetExceedsBounds(
        int pageNumber, int pageSize, string propertyName)
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery(new UsersFilter(), new PaginationParameters(pageNumber, pageSize), SortingParameters.None));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(propertyName);
    }

    [Theory(DisplayName = "Validate should accept pagination when page size is at a boundary")]
    [InlineData(1)]
    [InlineData(200)]
    public void Validate_Should_AcceptPagination_When_PageSizeIsAtABoundary(int pageSize)
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(
            new GetUsersQuery(new UsersFilter(), new PaginationParameters(1, pageSize), SortingParameters.None));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validate should reject pagination when pagination is null")]
    public void Validate_Should_RejectPagination_When_PaginationIsNull()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(new GetUsersQuery(new UsersFilter(), null!, SortingParameters.None));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("PaginationParameters");
    }

    [Fact(DisplayName = "Validate should reject filter when filter is null")]
    public void Validate_Should_RejectFilter_When_FilterIsNull()
    {
        var validator = new GetUsersQueryValidator();

        var result = validator.Validate(new GetUsersQuery(null!, new PaginationParameters(), SortingParameters.None));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Filter");
    }
}
