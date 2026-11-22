using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.GetList;

public sealed class GetFoldersQueryValidatorTests
{
    [Theory(DisplayName = "Folder list validator should accept query when type and paging are valid")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    [InlineData(" Personal ")]
    [InlineData(null)]
    public void Validate_Should_AcceptQuery_When_TypeAndPagingAreValid(string? type)
    {
        var query = new GetFoldersQuery(
            new FoldersFilter(AllowedFileType: type),
            new PaginationParameters(),
            SortingParameters.None);

        var result = new GetFoldersQueryValidator().Validate(query);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Folder list validator should reject type when type is invalid")]
    [InlineData("", "File type is required.")]
    [InlineData(" ", "File type is required.")]
    [InlineData("Unknown", "File type 'Unknown' is invalid.")]
    public void Validate_Should_RejectType_When_TypeIsInvalid(string type, string message)
    {
        var query = new GetFoldersQuery(
            new FoldersFilter(AllowedFileType: type),
            new PaginationParameters(),
            SortingParameters.None);

        var result = new GetFoldersQueryValidator().Validate(query);

        var error = result.Errors.ShouldHaveSingleItem();
        error.PropertyName.ShouldBe("Filter.AllowedFileType");
        error.ErrorMessage.ShouldBe(message);
    }

    [Fact(DisplayName = "Folder list validator should reject owner when user id is empty")]
    public void Validate_Should_RejectOwner_When_UserIdIsEmpty()
    {
        var query = new GetFoldersQuery(
            new FoldersFilter(UserId: Guid.Empty), new PaginationParameters(), SortingParameters.None);

        var result = new GetFoldersQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Filter.UserId");
    }

    [Theory(DisplayName = "Folder list validator should reject paging when limits are exceeded")]
    [InlineData(0, 10, "PaginationParameters.PageNumber")]
    [InlineData(1, 201, "PaginationParameters.PageSize")]
    [InlineData(int.MaxValue, 2, "PaginationParameters.PageNumber")]
    public void Validate_Should_RejectPaging_When_LimitsAreExceeded(int number, int size, string property)
    {
        var query = new GetFoldersQuery(
            new FoldersFilter(AllowedFileType: "Personal"),
            new PaginationParameters(number, size),
            SortingParameters.None);

        var result = new GetFoldersQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(property);
    }

    [Theory(DisplayName = "Folder list validator should reject sorting when field is not exposed")]
    [InlineData("StorageKey")]
    [InlineData("CreatedAt")]
    [InlineData("UserId")]
    public void Validate_Should_RejectSorting_When_FieldIsNotExposed(string field)
    {
        var query = new GetFoldersQuery(
            new FoldersFilter(AllowedFileType: "Personal"),
            new PaginationParameters(),
            SortingParameters.Ascending(field));

        var result = new GetFoldersQueryValidator().Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors
            .ShouldContain(error => error.PropertyName.StartsWith("SortingParameters", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Folder list validator should reject query when required parameters are absent")]
    [InlineData("Filter")]
    [InlineData("PaginationParameters")]
    [InlineData("SortingParameters")]
    public void Validate_Should_RejectQuery_When_RequiredParametersAreAbsent(string missing)
    {
        var query = new GetFoldersQuery(
            missing == "Filter" ? null! : new FoldersFilter(AllowedFileType: "Personal"),
            missing == "PaginationParameters" ? null! : new PaginationParameters(),
            missing == "SortingParameters" ? null! : SortingParameters.None);

        var result = new GetFoldersQueryValidator().Validate(query);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(missing);
    }
}
