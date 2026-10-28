using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetList;

public sealed class GetFilesQueryValidatorTests
{
    [Theory(DisplayName = "File list validator should accept query when type and paging are valid")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    [InlineData(" Personal ")]
    public void Validate_Should_AcceptQuery_When_TypeAndPagingAreValid(string type)
    {
        var query = new GetFilesQuery(new FilesFilter(type), new PaginationParameters(), SortingParameters.None);

        var result = new GetFilesQueryValidator().Validate(query);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "File list validator should reject type when type is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Validate_Should_RejectType_When_TypeIsInvalid(string type)
    {
        var query = new GetFilesQuery(new FilesFilter(type), new PaginationParameters(), SortingParameters.None);

        var result = new GetFilesQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Filter.Type");
    }

    [Theory(DisplayName = "File list validator should reject paging when limits are exceeded")]
    [InlineData(0, 10, "PaginationParameters.PageNumber")]
    [InlineData(1, 201, "PaginationParameters.PageSize")]
    [InlineData(int.MaxValue, 2, "PaginationParameters.PageNumber")]
    public void Validate_Should_RejectPaging_When_LimitsAreExceeded(int number, int size, string property)
    {
        var query = new GetFilesQuery(
            new FilesFilter("Personal"), new PaginationParameters(number, size), SortingParameters.None);

        var result = new GetFilesQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(property);
    }

    [Theory(DisplayName = "File list validator should reject sorting when field is not exposed")]
    [InlineData("StorageKey")]
    [InlineData("CreatedAt")]
    [InlineData("UserId")]
    public void Validate_Should_RejectSorting_When_FieldIsNotExposed(string field)
    {
        var query = new GetFilesQuery(
            new FilesFilter("Personal"), new PaginationParameters(), SortingParameters.Ascending(field));

        var result = new GetFilesQueryValidator().Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors
            .ShouldContain(error => error.PropertyName.StartsWith("SortingParameters", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "File list validator should reject query when required parameters are absent")]
    [InlineData("Filter")]
    [InlineData("PaginationParameters")]
    [InlineData("SortingParameters")]
    public void Validate_Should_RejectQuery_When_RequiredParametersAreAbsent(string missing)
    {
        var query = new GetFilesQuery(
            missing == "Filter" ? null! : new FilesFilter("Personal"),
            missing == "PaginationParameters" ? null! : new PaginationParameters(),
            missing == "SortingParameters" ? null! : SortingParameters.None);

        var result = new GetFilesQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(missing);
    }
}
