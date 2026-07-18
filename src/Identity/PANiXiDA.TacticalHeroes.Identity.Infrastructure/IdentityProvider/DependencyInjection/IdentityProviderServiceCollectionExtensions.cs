using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Common;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Providers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

internal static class IdentityProviderServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProvider(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IHostEnvironment? environment)
    {
        var identityProviderSection = configuration.GetSection(IdentityProviderOptions.SectionName);
        var identityProviderOptions = identityProviderSection.Get<IdentityProviderOptions>() ??
            new IdentityProviderOptions();

        serviceCollection.AddIdentityProviderOptionsValidators();
        serviceCollection
            .AddOptions<IdentityProviderOptions>()
            .Bind(identityProviderSection)
            .ValidateOnStart();

        serviceCollection.AddScoped<IUserCredentialsService, UserCredentialsService>();
        serviceCollection.AddScoped<IdentityProviderApplicationSeeder>();
        serviceCollection.AddHostedService<IdentityProviderApplicationSeederHostedService>();
        serviceCollection.AddDataProtection()
            .PersistKeysToDbContext<IdentityWriteDbContext>();

        serviceCollection
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = identityProviderOptions.User.RequireUniqueEmail;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                options.Password.RequiredLength = identityProviderOptions.Password.RequiredLength;
                options.Password.RequiredUniqueChars = identityProviderOptions.Password.RequiredUniqueChars;
                options.Password.RequireDigit = identityProviderOptions.Password.RequireDigit;
                options.Password.RequireLowercase = identityProviderOptions.Password.RequireLowercase;
                options.Password.RequireNonAlphanumeric = identityProviderOptions.Password.RequireNonAlphanumeric;
                options.Password.RequireUppercase = identityProviderOptions.Password.RequireUppercase;
                options.Lockout.MaxFailedAccessAttempts = identityProviderOptions.Lockout.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = identityProviderOptions.Lockout.DefaultLockoutTimeSpan;
                options.Lockout.AllowedForNewUsers = true;
                options.Tokens.EmailConfirmationTokenProvider = identityProviderOptions.TokenProviders.EmailConfirmation;
                options.Tokens.PasswordResetTokenProvider = identityProviderOptions.TokenProviders.PasswordReset;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityWriteDbContext>()
            .AddSignInManager()
            .AddTokenProvider<EmailConfirmationTokenProvider>(identityProviderOptions.TokenProviders.EmailConfirmation)
            .AddTokenProvider<PasswordResetTokenProvider>(identityProviderOptions.TokenProviders.PasswordReset)
            .AddDefaultTokenProviders();

        serviceCollection.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityWriteDbContext>()
                    .ReplaceDefaultEntities<Guid>();

                options.UseQuartz();
            })
            .AddServer(options =>
            {
                if (identityProviderOptions.Issuer is not null)
                {
                    options.SetIssuer(identityProviderOptions.Issuer);
                }

                options.SetPushedAuthorizationEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.PushedAuthorization));
                options.SetAuthorizationEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.Authorization));
                options.SetTokenEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.Token));
                options.SetUserInfoEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.UserInfo));
                options.SetEndSessionEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.EndSession));
                options.SetIntrospectionEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.Introspection));
                options.SetRevocationEndpointUris(
                    OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.Revocation));

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                options.RequirePushedAuthorizationRequests();
                options.AllowClientCredentialsFlow();
                options.AllowRefreshTokenFlow();
                options.AllowTokenExchangeFlow();
                options.RegisterScopes(
                [
                    .. new[]
                    {
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.OfflineAccess,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles
                    }
                    .Concat(identityProviderOptions.Clients.SelectMany(client => client.Scopes))
                    .Where(scope => !string.IsNullOrWhiteSpace(scope))
                    .Distinct(StringComparer.Ordinal)
                ]);
                options.SetAccessTokenLifetime(identityProviderOptions.AccessTokenLifetime);
                options.SetRefreshTokenLifetime(identityProviderOptions.RefreshTokenLifetime);
                options.SetRefreshTokenReuseLeeway(identityProviderOptions.RefreshTokenReuseLeeway);
                options.SetAuthorizationCodeLifetime(identityProviderOptions.AuthorizationCodeLifetime);
                options.SetIdentityTokenLifetime(identityProviderOptions.IdentityTokenLifetime);
                options.UseReferenceAccessTokens();
                options.UseReferenceRefreshTokens();
                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                if (environment is null ||
                    environment.IsDevelopment() ||
                    environment.IsEnvironment(EnvironmentConstants.Test))
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.EnableTokenEntryValidation();
            });

        serviceCollection
            .AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .AddIdentityCookies();
        serviceCollection.AddAuthorization();

        return serviceCollection;
    }
}
