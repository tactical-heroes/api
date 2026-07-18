using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.ValueObjects;

public sealed class UserConfirmationStatusTests
{
    [Fact(DisplayName = "Unconfirmed should create an unconfirmed status")]
    public void Unconfirmed_Should_CreateStatus_When_Called()
    {
        var status = UserConfirmationStatus.Unconfirmed();

        status.IsConfirmed.ShouldBeFalse();
    }

    [Fact(DisplayName = "Confirmed should create a confirmed status")]
    public void Confirmed_Should_CreateStatus_When_Called()
    {
        var status = UserConfirmationStatus.Confirmed();

        status.IsConfirmed.ShouldBeTrue();
    }

    [Theory(DisplayName = "From should create a status from a boolean value")]
    [InlineData(true)]
    [InlineData(false)]
    public void From_Should_CreateStatus_When_BooleanIsProvided(bool isConfirmed)
    {
        var status = UserConfirmationStatus.From(isConfirmed);

        status.IsConfirmed.ShouldBe(isConfirmed);
    }
}
