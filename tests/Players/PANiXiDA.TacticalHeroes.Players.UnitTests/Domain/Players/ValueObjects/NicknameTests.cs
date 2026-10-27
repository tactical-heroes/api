using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Domain.Players.ValueObjects;

public sealed class NicknameTests
{
    [Theory(DisplayName = "Nickname should preserve spelling and trim whitespace when value is valid")]
    [InlineData("  Commander  ", "Commander")]
    [InlineData("  Командир  ", "Командир")]
    [InlineData("a", "a")]
    public void Create_Should_TrimAndPreserveSpelling_When_ValueIsValid(string value, string expected)
    {
        var result = Nickname.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Theory(DisplayName = "Nickname should reject empty input when value is blank")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsBlank(string? value)
    {
        var result = Nickname.Create(value!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Nickname cannot be empty.")
            .ShouldHaveField(nameof(Nickname));
    }

    [Fact(DisplayName = "Nickname should accept trimmed input when length is at the maximum")]
    public void Create_Should_AcceptTrimmedValue_When_LengthIsAtTheMaximum()
    {
        var value = new string('a', Nickname.MaxLength);

        var result = Nickname.Create($"  {value}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Nickname should reject input when length exceeds the maximum")]
    public void Create_Should_ReturnValidationFailure_When_LengthExceedsTheMaximum()
    {
        var result = Nickname.Create(new string('a', Nickname.MaxLength + 1));

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            $"Nickname cannot be longer than {Nickname.MaxLength} characters.")
            .ShouldHaveField(nameof(Nickname));
    }

    [Fact(DisplayName = "Nickname should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var nickname = Nickname.Create("Commander").Value;

        var result = nickname.ToString();

        result.ShouldBe("Nickname { Value = Commander }");
    }
}
