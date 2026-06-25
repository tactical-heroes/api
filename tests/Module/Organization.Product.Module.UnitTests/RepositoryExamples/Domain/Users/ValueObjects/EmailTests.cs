using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.UnitTests.Domain.Users.ValueObjects;

public sealed class EmailTests
{
    [Fact(DisplayName = "Create should normalize email when value has uppercase and whitespace")]
    public void Create_Should_Normalize_Email_When_Value_Has_Uppercase_And_Whitespace()
    {
        var value = "  User.Name@Example.COM  ";

        var result = Email.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("user.name@example.com");
        result.Value.ToString().ShouldBe("user.name@example.com");
    }

    [Fact(DisplayName = "Create should return failure when email is empty")]
    public void Create_Should_Return_Failure_When_Email_Is_Empty()
    {
        var value = " ";

        var result = Email.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Email cannot be empty.");

        error.ShouldHaveField(nameof(Email));
    }

    [Fact(DisplayName = "Create should return failure when email is too long")]
    public void Create_Should_Return_Failure_When_Email_Is_Too_Long()
    {
        var value = $"{new string('a', 309)}@example.com";

        var result = Email.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Email cannot be longer than 320 characters.");

        error.ShouldHaveField(nameof(Email));
    }

    [Theory(DisplayName = "Create should return failure when email format is invalid")]
    [InlineData("invalid-email")]
    [InlineData("John Doe <john@example.com>")]
    public void Create_Should_Return_Failure_When_Email_Format_Is_Invalid(string email)
    {
        var result = Email.Create(email);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Email has invalid format.");

        error.ShouldHaveField(nameof(Email));
    }

    [Fact(DisplayName = "Equals should compare by normalized value when emails differ by case")]
    public void Equals_Should_Compare_By_Normalized_Value_When_Emails_Differ_By_Case()
    {
        var first = Email.Create("USER@example.com").Value;
        var second = Email.Create("user@example.com").Value;

        first.ShouldBe(second);
    }
}
