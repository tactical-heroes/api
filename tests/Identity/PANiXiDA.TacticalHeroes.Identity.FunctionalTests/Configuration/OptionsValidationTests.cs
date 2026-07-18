using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Lockout;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Password;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.TokenProviders;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Configuration;

public sealed class OptionsValidationTests
{
    [Fact(DisplayName = "Identity options validators should accept valid configuration")]
    public void Validate_Should_AcceptValidConfiguration()
    {
        var providerResult = ValidateIdentityProviderOptions(
            options: new IdentityProviderOptions
            {
                Issuer = new Uri(uriString: "https://localhost:7091/"),
                Audience = "tactical-heroes-api",
                Clients =
                [
                    new IdentityProviderClientOptions
                    {
                        ClientId = "web",
                        DisplayName = "Web",
                        GrantTypes = [OpenIddictConstants.GrantTypes.AuthorizationCode]
                    }
                ]
            });
        var messagingResult = new IdentityMessagingOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new IdentityMessagingOptions());
        var cleanupResult = new IdentityCleanupOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new IdentityCleanupOptions());
        var spaResult = new OAuthSpaOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new OAuthSpaOptions
            {
                LoginUrl = "https://localhost:5173/login"
            });
        var tokenResult = new OAuthTokenOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new OAuthTokenOptions
            {
                Audience = "tactical-heroes-api"
            });

        providerResult.Succeeded.ShouldBeTrue();
        messagingResult.Succeeded.ShouldBeTrue();
        cleanupResult.Succeeded.ShouldBeTrue();
        spaResult.Succeeded.ShouldBeTrue();
        tokenResult.Succeeded.ShouldBeTrue();
    }

    [Fact(DisplayName = "Identity provider options validator should reject invalid configuration")]
    public void Validate_Should_RejectInvalidIdentityProviderConfiguration()
    {
        var result = ValidateIdentityProviderOptions(
            options: new IdentityProviderOptions
            {
                Audience = string.Empty,
                AccessTokenLifetime = TimeSpan.Zero,
                RefreshTokenReuseLeeway = TimeSpan.FromSeconds(seconds: -1),
                Password = new IdentityProviderPasswordOptions
                {
                    RequiredLength = 0,
                    RequiredUniqueChars = 1
                },
                Lockout = new IdentityProviderLockoutOptions
                {
                    MaxFailedAccessAttempts = 0,
                    DefaultLockoutTimeSpan = TimeSpan.Zero
                },
                TokenProviders = new IdentityProviderTokenProviderOptions
                {
                    EmailConfirmation = "shared",
                    PasswordReset = "shared"
                },
                Clients =
                [
                    new IdentityProviderClientOptions
                    {
                        ClientId = "service",
                        ClientType = OpenIddictConstants.ClientTypes.Confidential,
                        ClientSecret = string.Empty,
                        RedirectUris = ["relative/callback"]
                    }
                ]
            });

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(failure => failure.Contains("Issuer", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("Audience", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("AccessTokenLifetime", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("RefreshTokenReuseLeeway", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("RequiredLength", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("MaxFailedAccessAttempts", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("must be unique", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("ClientSecret", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("absolute URIs", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Identity messaging options validator should reject invalid templates")]
    public void Validate_Should_RejectInvalidIdentityMessagingConfiguration()
    {
        var result = new IdentityMessagingOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new IdentityMessagingOptions
            {
                EmailConfirmationUrlTemplate = "/confirm/{userId}",
                PasswordResetUrlTemplate = "reset/{token}"
            });

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(failure => failure.Contains("{token}", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("{userId}", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("root-relative", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Identity cleanup options validator should reject invalid schedule")]
    public void Validate_Should_RejectInvalidIdentityCleanupConfiguration()
    {
        var validator = new IdentityCleanupOptionsValidator();
        var result = validator.Validate(
            name: Options.DefaultName,
            options: new IdentityCleanupOptions
            {
                PruneUnconfirmedUsersEnabled = true,
                UnconfirmedUserRetention = TimeSpan.Zero,
                UnconfirmedUsersCronSchedule = "invalid"
            });
        var disabledResult = validator.Validate(
            name: Options.DefaultName,
            options: new IdentityCleanupOptions
            {
                PruneUnconfirmedUsersEnabled = false,
                UnconfirmedUserRetention = TimeSpan.Zero,
                UnconfirmedUsersCronSchedule = "invalid"
            });

        result.Failed.ShouldBeTrue();
        disabledResult.Succeeded.ShouldBeTrue();
        result.Failures.ShouldContain(failure => failure.Contains("UnconfirmedUserRetention", StringComparison.Ordinal));
        result.Failures.ShouldContain(failure => failure.Contains("cron", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "OAuth options validators should reject invalid configuration")]
    public void Validate_Should_RejectInvalidOAuthConfiguration()
    {
        var spaResult = new OAuthSpaOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new OAuthSpaOptions
            {
                LoginUrl = "/login"
            });
        var tokenResult = new OAuthTokenOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new OAuthTokenOptions
            {
                Audience = string.Empty
            });

        spaResult.Failed.ShouldBeTrue();
        tokenResult.Failed.ShouldBeTrue();
    }

    private static ValidateOptionsResult ValidateIdentityProviderOptions(
        IdentityProviderOptions options)
    {
        var services = new ServiceCollection();
        services.AddIdentityProviderOptionsValidators();

        using var serviceProvider = services.BuildServiceProvider();
        List<string> failures = [];

        foreach (var validator in serviceProvider.GetServices<IValidateOptions<IdentityProviderOptions>>())
        {
            var result = validator.Validate(
                name: Options.DefaultName,
                options: options);

            if (result.Failed)
            {
                failures.AddRange(collection: result.Failures);
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
