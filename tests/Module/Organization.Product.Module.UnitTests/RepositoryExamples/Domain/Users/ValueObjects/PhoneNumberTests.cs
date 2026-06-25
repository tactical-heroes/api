using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.UnitTests.Domain.Users.ValueObjects;

public sealed class PhoneNumberTests
{
    [Fact(DisplayName = "Create should normalize phone number when value has formatting characters")]
    public void Create_Should_Normalize_Phone_Number_When_Value_Has_Formatting_Characters()
    {
        var value = "  +1 (234) 567-8901  ";

        var result = PhoneNumber.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("+12345678901");
        result.Value.ToString().ShouldBe("+12345678901");
    }

    [Fact(DisplayName = "Create should ignore plus when it is not first normalized character")]
    public void Create_Should_Ignore_Plus_When_It_Is_Not_First_Normalized_Character()
    {
        var value = "1+234-5678";

        var result = PhoneNumber.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("12345678");
    }

    [Fact(DisplayName = "Create should return failure when phone number is empty")]
    public void Create_Should_Return_Failure_When_Phone_Number_Is_Empty()
    {
        var value = " ";

        var result = PhoneNumber.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Phone number cannot be empty.");

        error.ShouldHaveField(nameof(PhoneNumber));
    }

    [Fact(DisplayName = "Create should return failure when phone number has no digits or plus")]
    public void Create_Should_Return_Failure_When_Phone_Number_Has_No_Digits_Or_Plus()
    {
        var value = "phone";

        var result = PhoneNumber.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Phone number has invalid format.");

        error.ShouldHaveField(nameof(PhoneNumber));
    }

    [Theory(DisplayName = "Create should return failure when digits count is out of range")]
    [InlineData("+1234567")]
    [InlineData("+1234567890123456")]
    public void Create_Should_Return_Failure_When_Digits_Count_Is_Out_Of_Range(
        string phone)
    {
        var result = PhoneNumber.Create(phone);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Phone number must contain from 8 to 15 digits.");

        error.ShouldHaveField(nameof(PhoneNumber));
    }

    [Fact(DisplayName = "Equals should compare by normalized value when phone numbers have different formatting")]
    public void Equals_Should_Compare_By_Normalized_Value_When_Phone_Numbers_Have_Different_Formatting()
    {
        var first = PhoneNumber.Create("+1 (234) 567-8901").Value;
        var second = PhoneNumber.Create("+12345678901").Value;

        first.ShouldBe(second);
    }
}
