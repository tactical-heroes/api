using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Common.Filters;

public sealed class FoldersFilterValidatorTests
{
    [Theory(DisplayName = "Validate should reject search when trimmed search is shorter than three characters")]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("no")]
    [InlineData("   ")]
    [InlineData(" no ")]
    public void Validate_Should_RejectSearch_When_TrimmedSearchIsShorterThanThreeCharacters(string search)
    {
        var validator = new FoldersFilterValidator();

        var result = validator.Validate(new FoldersFilter(search));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(FoldersFilter.Search));
    }

    [Fact(DisplayName = "Validate should reject search when search exceeds maximum length")]
    public void Validate_Should_RejectSearch_When_SearchExceedsMaximumLength()
    {
        var validator = new FoldersFilterValidator();

        var result = validator.Validate(new FoldersFilter(new string('a', 256)));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(FoldersFilter.Search));
    }

    [Theory(DisplayName = "Validate should accept filter when search is omitted or valid")]
    [InlineData(null)]
    [InlineData("nor")]
    [InlineData(" north ")]
    public void Validate_Should_AcceptFilter_When_SearchIsOmittedOrValid(string? search)
    {
        var validator = new FoldersFilterValidator();

        var result = validator.Validate(new FoldersFilter(search));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Validate should accept search when trimmed length is at boundary")]
    [InlineData(3)]
    [InlineData(255)]
    public void Validate_Should_AcceptSearch_When_TrimmedLengthIsAtBoundary(int length)
    {
        var validator = new FoldersFilterValidator();
        var search = " " + new string('a', length) + " ";

        var result = validator.Validate(new FoldersFilter(search));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Folder filter should validate allowed type when optional type varies")]
    [InlineData(null, true)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", true)]
    [InlineData(" Personal ", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void Validate_Should_ValidateAllowedType_When_OptionalTypeVaries(string? type, bool valid)
    {
        var filter = new FoldersFilter(AllowedFileType: type);

        var result = new FoldersFilterValidator().Validate(filter);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(filter.AllowedFileType));
        }
    }

    [Theory(DisplayName = "Folder filter should validate owner when user id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateOwner_When_UserIdValidityVaries(bool valid)
    {
        var filter = new FoldersFilter(UserId: valid ? Guid.NewGuid() : Guid.Empty);

        var result = new FoldersFilterValidator().Validate(filter);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(filter.UserId));
        }
    }
}
