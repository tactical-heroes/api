using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.EmailRequestCooldown;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class EmailRequestCooldownOptionsTests
{
    [Theory(DisplayName = "Cooldown should fail startup validation when intervals or cleanup schedule are invalid")]
    [InlineData("Confirmation", "00:00:00")]
    [InlineData("AccountRecovery", "-00:00:01")]
    [InlineData("CleanupCronSchedule", "invalid")]
    [InlineData("CleanupCronSchedule", "")]
    public void Validate_Should_FailStartup_When_IntervalsOrCleanupScheduleAreInvalid(string key, string value)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"Identity:EmailRequestCooldown:{key}"] = value
        })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IValidateOptions<EmailRequestCooldownOptions>, EmailRequestCooldownOptionsValidator>();
        services.AddOptions<EmailRequestCooldownOptions>()
            .Bind(configuration.GetSection(EmailRequestCooldownOptions.SectionName))
            .ValidateOnStart();
        using var provider = services.BuildServiceProvider();

        var exception = Should.Throw<OptionsValidationException>(() =>
            provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Failures.ShouldNotBeEmpty();
    }
}
