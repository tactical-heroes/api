using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetList;

public sealed class GetFactionsQueryValidatorTests
{
    [Fact(DisplayName = "Factions validator should reject invalid pagination when pagination is invalid")]
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

    [Theory(DisplayName = "Validate should apply shared pagination rules when page size or offset exceeds bounds")]
    [InlineData(1, 201, "Pagination.PageSize")]
    [InlineData(int.MaxValue, 2, "Pagination.PageNumber")]
    public void Validate_Should_ApplySharedPaginationRules_When_PageSizeOrOffsetExceedsBounds(
        int pageNumber, int pageSize, string propertyName)
    {
        var validator = new GetFactionsQueryValidator();

        var result = validator.Validate(
            new GetFactionsQuery(new PaginationParameters(pageNumber, pageSize)));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(propertyName);
    }

    [Theory(DisplayName = "Validate should accept pagination when page size is at a boundary")]
    [InlineData(1)]
    [InlineData(200)]
    public void Validate_Should_AcceptPagination_When_PageSizeIsAtABoundary(int pageSize)
    {
        var validator = new GetFactionsQueryValidator();

        var result = validator.Validate(
            new GetFactionsQuery(new PaginationParameters(1, pageSize)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validate should reject pagination when pagination is null")]
    public void Validate_Should_RejectPagination_When_PaginationIsNull()
    {
        var validator = new GetFactionsQueryValidator();

        var result = validator.Validate(new GetFactionsQuery(null!));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Pagination");
    }
}
