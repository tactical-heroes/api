using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Configuration;

public sealed class OptionsValidationTests
{
    [Fact(DisplayName = "Identity options validators should accept valid configuration")]
    public void Validate_Should_AcceptValidConfiguration()
    {
        var providerResult = new IdentityProviderOptionsValidator().Validate(
            name: Options.DefaultName,
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
        var result = new IdentityProviderOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new IdentityProviderOptions
            {
                Audience = string.Empty,
                AccessTokenLifetime = TimeSpan.Zero,
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
        var result = new IdentityCleanupOptionsValidator().Validate(
            name: Options.DefaultName,
            options: new IdentityCleanupOptions
            {
                UnconfirmedUserRetention = TimeSpan.Zero,
                UnconfirmedUsersCronSchedule = "invalid"
            });

        result.Failed.ShouldBeTrue();
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
}
