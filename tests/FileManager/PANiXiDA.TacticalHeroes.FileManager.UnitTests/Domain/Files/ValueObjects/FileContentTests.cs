using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileContentTests
{
    [Fact(DisplayName = "File content should normalize metadata when values are valid")]
    public void Create_Should_NormalizeMetadata_When_ValuesAreValid()
    {
        var result = FileContent.Create(
            " IMAGE/PNG ",
            1,
            new string('A', 64));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ContentType.Value.ShouldBe("image/png");
        result.Value.Size.ShouldBe(1);
        result.Value.Sha256.Value.ShouldBe(new string('a', 64));
    }

    [Theory(DisplayName = "File content should reject nonpositive sizes when size is not positive")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_Should_ReturnValidationFailure_When_SizeIsNotPositive(long size)
    {
        var result = FileContent.Create(
            "image/png",
            size,
            new string('a', 64));

        result.ShouldHaveSingleError(ErrorType.Validation, "File size must be greater than zero.")
            .ShouldHaveField(nameof(FileContent.Size));
    }

    [Fact(DisplayName = "File content should report all invalid fields when multiple fields are invalid")]
    public void Create_Should_ReturnAllErrors_When_MultipleFieldsAreInvalid()
    {
        var result = FileContent.Create(
            "",
            0,
            "");

        result.Errors.Count.ShouldBe(3);
    }

    [Fact(DisplayName = "File content should format its metadata when converted to string")]
    public void ToString_Should_FormatMetadata_When_ConvertedToString()
    {
        var content = FileContent.Create(
            "image/png",
            128,
            new string('a', 64)).Value;

        var result = content.ToString();

        result.ShouldContain($"ContentType = {content.ContentType}");
        result.ShouldContain("Size = 128");
        result.ShouldContain($"Sha256 = {content.Sha256}");
    }
}
