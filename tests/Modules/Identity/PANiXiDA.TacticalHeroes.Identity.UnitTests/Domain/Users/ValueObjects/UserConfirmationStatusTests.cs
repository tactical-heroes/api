using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.ValueObjects;

public sealed class UserConfirmationStatusTests
{
    [Theory(DisplayName = "User confirmation status should format its value when converted to string")]
    [InlineData(true, "UserConfirmationStatus { IsConfirmed = True }")]
    [InlineData(false, "UserConfirmationStatus { IsConfirmed = False }")]
    public void ToString_Should_FormatValue_When_ConvertedToString(bool isConfirmed, string expected)
    {
        var status = UserConfirmationStatus.From(isConfirmed);

        var result = status.ToString();

        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "Unconfirmed should create an unconfirmed status when called")]
    public void Unconfirmed_Should_CreateStatus_When_Called()
    {
        var status = UserConfirmationStatus.Unconfirmed();

        status.IsConfirmed.ShouldBeFalse();
    }

    [Fact(DisplayName = "Confirmed should create a confirmed status when called")]
    public void Confirmed_Should_CreateStatus_When_Called()
    {
        var status = UserConfirmationStatus.Confirmed();

        status.IsConfirmed.ShouldBeTrue();
    }

    [Theory(DisplayName = "From should create a status from a boolean value when boolean is provided")]
    [InlineData(true)]
    [InlineData(false)]
    public void From_Should_CreateStatus_When_BooleanIsProvided(bool isConfirmed)
    {
        var status = UserConfirmationStatus.From(isConfirmed);

        status.IsConfirmed.ShouldBe(isConfirmed);
    }
}
