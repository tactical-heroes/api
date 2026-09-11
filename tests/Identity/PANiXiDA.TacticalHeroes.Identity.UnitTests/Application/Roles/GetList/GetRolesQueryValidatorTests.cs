using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetList;

public sealed class GetRolesQueryValidatorTests
{
    [Fact(DisplayName = "Role list validator should accept valid pagination when pagination is valid")]
    public void Validate_Should_ReturnValidResult_When_PaginationIsValid()
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(new GetRolesQuery(new PaginationParameters(1, 20)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Role list validator should reject invalid pagination when pagination is invalid")]
    public void Validate_Should_ReturnErrors_When_PaginationIsInvalid()
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(new GetRolesQuery(new PaginationParameters(0, 0)));

        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }

    [Theory(DisplayName = "Validate should apply shared pagination rules when page size or offset exceeds bounds")]
    [InlineData(1, 201, "Pagination.PageSize")]
    [InlineData(int.MaxValue, 2, "Pagination.PageNumber")]
    public void Validate_Should_ApplySharedPaginationRules_When_PageSizeOrOffsetExceedsBounds(
        int pageNumber, int pageSize, string propertyName)
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(
            new GetRolesQuery(new PaginationParameters(pageNumber, pageSize)));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(propertyName);
    }

    [Theory(DisplayName = "Validate should accept pagination when page size is at a boundary")]
    [InlineData(1)]
    [InlineData(200)]
    public void Validate_Should_AcceptPagination_When_PageSizeIsAtABoundary(int pageSize)
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(
            new GetRolesQuery(new PaginationParameters(1, pageSize)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validate should reject pagination when pagination is null")]
    public void Validate_Should_RejectPagination_When_PaginationIsNull()
    {
        var validator = new GetRolesQueryValidator();

        var result = validator.Validate(new GetRolesQuery(null!));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Pagination");
    }
}
