using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.ValueObjects;

public sealed class EmailTests
{
    [Fact(DisplayName = "Email should normalize a valid value")]
    public void Create_Should_NormalizeValue_When_EmailIsValid()
    {
        var result = Email.Create("  HERO@Example.COM  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("hero@example.com");
        result.Value.ToString().ShouldBe("hero@example.com");
    }

    [Theory(DisplayName = "Email should reject an empty value")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_EmailIsEmpty(string value)
    {
        var result = Email.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Email cannot be empty.")
            .ShouldHaveField(nameof(Email));
    }

    [Theory(DisplayName = "Email should reject an invalid format")]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public void Create_Should_ReturnValidationFailure_When_EmailFormatIsInvalid(string value)
    {
        var result = Email.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Email has invalid format.")
            .ShouldHaveField(nameof(Email));
    }

    [Fact(DisplayName = "Email should reject a value over the maximum length")]
    public void Create_Should_ReturnValidationFailure_When_EmailIsTooLong()
    {
        var result = Email.Create($"{new string('a', Email.MaxLength)}@example.com");

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Email cannot be longer than {Email.MaxLength} characters.")
            .ShouldHaveField(nameof(Email));
    }
}
