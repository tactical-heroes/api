using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.UnitTests.Domain.Users.ValueObjects;

public sealed class BirthDateTests
{
    [Fact(DisplayName = "Create should return birth date when user is at least 18")]
    public void Create_Should_Return_Birth_Date_When_User_Is_At_Least_18()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18);

        var result = BirthDate.Create(birthDate);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(birthDate);
        result.Value.ToString().ShouldBe(birthDate.ToString("yyyy-MM-dd"));
    }

    [Fact(DisplayName = "Create should return failure when birth date is in future")]
    public void Create_Should_Return_Failure_When_Birth_Date_Is_In_Future()
    {
        var value = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var result = BirthDate.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Birth date cannot be in the future.");

        error.ShouldHaveField(nameof(BirthDate));
    }

    [Fact(DisplayName = "Create should return failure when user is under 18")]
    public void Create_Should_Return_Failure_When_User_Is_Under_18()
    {
        var value = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17);

        var result = BirthDate.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User must be at least 18 years old.");

        error.ShouldHaveField(nameof(BirthDate));
    }

    [Theory(DisplayName = "GetAge should calculate age when date is on or after birth date")]
    [InlineData(1990, 6, 14, 2026, 6, 14, 36)]
    [InlineData(1990, 6, 15, 2026, 6, 14, 35)]
    [InlineData(1990, 1, 1, 2026, 6, 14, 36)]
    [InlineData(1990, 12, 1, 2026, 6, 14, 35)]
    public void GetAge_Should_Calculate_Age_When_Date_Is_On_Or_After_Birth_Date(
        int birthYear,
        int birthMonth,
        int birthDay,
        int onYear,
        int onMonth,
        int onDay,
        int expectedAge)
    {
        var birthDate = BirthDate.Create(new DateOnly(birthYear, birthMonth, birthDay)).Value;

        var age = birthDate.GetAge(new DateOnly(onYear, onMonth, onDay));

        age.ShouldBe(expectedAge);
    }

    [Fact(DisplayName = "GetAge should throw when calculation date is earlier than birth date")]
    public void GetAge_Should_Throw_When_Calculation_Date_Is_Earlier_Than_Birth_Date()
    {
        var birthDate = BirthDate.Create(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30)).Value;

        Should.Throw<ArgumentOutOfRangeException>(
            () => birthDate.GetAge(birthDate.Value.AddDays(-1)));
    }

    [Theory(DisplayName = "IsAtLeast should return whether user reached age when calculation date is valid")]
    [InlineData(21, true)]
    [InlineData(31, false)]
    public void IsAtLeast_Should_Return_Whether_User_Reached_Age_When_Calculation_Date_Is_Valid(
        int requiredAge,
        bool expected)
    {
        var birthDate = BirthDate.Create(new DateOnly(2000, 1, 1)).Value;

        var result = birthDate.IsAtLeast(requiredAge, new DateOnly(2021, 1, 1));

        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "Equals should compare by value when birth dates are equal")]
    public void Equals_Should_Compare_By_Value_When_Birth_Dates_Are_Equal()
    {
        var first = BirthDate.Create(new DateOnly(2000, 1, 1)).Value;
        var second = BirthDate.Create(new DateOnly(2000, 1, 1)).Value;

        first.ShouldBe(second);
    }
}
