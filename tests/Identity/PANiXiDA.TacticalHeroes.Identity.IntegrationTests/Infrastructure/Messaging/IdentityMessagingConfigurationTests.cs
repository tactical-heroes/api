using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Builders;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options.IdentityMessaging;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Messaging;

public sealed class IdentityMessagingConfigurationTests
{
    [Theory(DisplayName = "Validate should reject invalid email links at startup when URL templates are invalid")]
    [InlineData("")]
    [InlineData("/api/v1/auth/reset-password?userId={userId}&token={token}")]
    [InlineData("//example.test/reset-password?userId={userId}&token={token}")]
    [InlineData("http:///reset-password?userId={userId}&token={token}")]
    [InlineData("ftp://example.test/reset-password?userId={userId}&token={token}")]
    [InlineData("https://example.test/reset-password?token={token}")]
    [InlineData("https://example.test/reset-password?userId={userId}")]
    public void Validate_Should_RejectInvalidEmailLinksAtStartup_When_UrlTemplatesAreInvalid(string template)
    {
        using var services = CreateServices(template, template);

        var exception = Should.Throw<OptionsValidationException>(() =>
            services.GetRequiredService<IStartupValidator>().Validate());

        exception.Failures.ShouldContain(failure => failure.Contains(
            "Identity:Messaging:EmailConfirmationUrlTemplate", StringComparison.Ordinal));
        exception.Failures.ShouldContain(failure => failure.Contains(
            "Identity:Messaging:PasswordResetUrlTemplate", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Validate should reject missing email links at startup when templates are not configured")]
    public void Validate_Should_RejectMissingEmailLinksAtStartup_When_TemplatesAreNotConfigured()
    {
        using var services = CreateServices(null, null);

        Should.Throw<OptionsValidationException>(() =>
            services.GetRequiredService<IStartupValidator>().Validate());
    }

    [Theory(DisplayName = "Build should preserve query values when configured email links contain encoded tokens")]
    [InlineData("https://dev.tactical-heroes.panixida.ru")]
    [InlineData("https://tactical-heroes.panixida.ru")]
    [InlineData("https://localhost:5173")]
    [InlineData("http://localhost:5084")]
    public void Build_Should_PreserveQueryValues_When_ConfiguredEmailLinksContainEncodedTokens(string origin)
    {
        using var services = CreateServices(
            origin + "/confirm-email?userId={userId}&emailConfirmationToken={token}",
            origin + "/reset-password?userId={userId}&passwordResetToken={token}");
        services.GetRequiredService<IStartupValidator>().Validate();
        var options = services.GetRequiredService<IOptions<IdentityMessagingOptions>>().Value;
        var userId = Guid.NewGuid();
        const string token = "token/+==&?% value";

        var confirmationUrl = new Uri(IdentityLinkBuilder.Build(options.EmailConfirmationUrlTemplate, userId, token));
        var passwordResetUrl = new Uri(IdentityLinkBuilder.Build(options.PasswordResetUrlTemplate, userId, token));

        confirmationUrl.GetLeftPart(UriPartial.Authority).ShouldBe(origin);
        confirmationUrl.AbsolutePath.ShouldBe("/confirm-email");
        var confirmationQuery = QueryHelpers.ParseQuery(confirmationUrl.Query);
        confirmationQuery["userId"].ToString().ShouldBe(userId.ToString("D"));
        confirmationQuery["emailConfirmationToken"].ToString().ShouldBe(token);
        passwordResetUrl.GetLeftPart(UriPartial.Authority).ShouldBe(origin);
        passwordResetUrl.AbsolutePath.ShouldBe("/reset-password");
        var passwordResetQuery = QueryHelpers.ParseQuery(passwordResetUrl.Query);
        passwordResetQuery["userId"].ToString().ShouldBe(userId.ToString("D"));
        passwordResetQuery["passwordResetToken"].ToString().ShouldBe(token);
    }

    private static ServiceProvider CreateServices(string? confirmationTemplate, string? passwordResetTemplate)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Identity:Messaging:EmailConfirmationUrlTemplate"] = confirmationTemplate,
                ["Identity:Messaging:PasswordResetUrlTemplate"] = passwordResetTemplate
            }.Where(pair => pair.Value is not null))
            .Build();
        var services = new ServiceCollection();
        services.AddMessaging(configuration);

        return services.BuildServiceProvider();
    }
}
