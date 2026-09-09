using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.ValueObjects;

public sealed class UserActionTokenTests
{
    [Fact(DisplayName = "User action token should preserve the opaque value and expiration when value is valid")]
    public void Create_Should_PreserveValueAndExpiration_When_ValueIsValid()
    {
        var expiresAtUtc = new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

        var result = UserActionToken.Create(" opaque-token+/= ", expiresAtUtc);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(" opaque-token+/= ");
        result.Value.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Theory(DisplayName = "User action token should reject missing values when value is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty(string? value)
    {
        var result = UserActionToken.Create(value!, DateTimeOffset.UtcNow);

        result.ShouldHaveSingleError(ErrorType.Validation, "User action token cannot be empty.");
    }
}
