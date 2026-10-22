using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Common;

public sealed class AuthRateLimitOptionsTests
{
    [Theory(DisplayName = "Auth rate limits should fail startup validation when configured policy is invalid")]
    [InlineData("Login:PermitLimit", "0")]
    [InlineData("Register:Window", "00:00:00")]
    [InlineData("ForgotPassword:PermitLimit", "-1")]
    [InlineData("ResetPassword:Window", "-00:00:01")]
    [InlineData("ResendConfirmationEmail:PermitLimit", "0")]
    [InlineData("ConfirmEmail:Window", "00:00:00")]
    [InlineData("ChangePassword:PermitLimit", "0")]
    public void Validate_Should_FailStartup_When_ConfiguredPolicyIsInvalid(string key, string value)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"Identity:RateLimits:{key}"] = value
        })
            .Build();
        var services = new ServiceCollection();
        services.AddAuthRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();

        var exception = Should.Throw<OptionsValidationException>(() =>
            provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Failures.ShouldNotBeEmpty();
    }
}
