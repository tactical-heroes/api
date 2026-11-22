using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Logout;

public sealed class LogoutCommandValidatorTests
{
    [Theory(DisplayName = "Logout validator should require session identity when command fields vary")]
    [InlineData("valid")]
    [InlineData("user")]
    [InlineData("authorization")]
    [InlineData("client")]
    public void Validate_Should_RequireSessionIdentity_When_CommandFieldsVary(string mode)
    {
        var validator = new LogoutCommandValidator();
        var command = new LogoutCommand(
            mode == "user" ? Guid.Empty : Guid.CreateVersion7(),
            mode == "authorization" ? "" : Guid.CreateVersion7().ToString(),
            mode == "client" ? "" : "client");

        var result = validator.Validate(command);

        result.IsValid.ShouldBe(mode == "valid");
    }
}
