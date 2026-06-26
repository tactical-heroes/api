using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

internal static class IdentityProviderServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProvider(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var identityProviderOptions = configuration
            .GetSection(IdentityProviderOptions.SectionName)
            .Get<IdentityProviderOptions>() ?? new IdentityProviderOptions();

        serviceCollection.Configure<IdentityProviderOptions>(
            configuration.GetSection(IdentityProviderOptions.SectionName));

        serviceCollection.AddScoped<IPasswordHashingService, PasswordHashingService>();
        serviceCollection.AddScoped<IUserTokenService, UserTokenService>();
        serviceCollection.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
        serviceCollection.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        serviceCollection.AddScoped<IdentityProviderApplicationSeeder>();
        serviceCollection.AddHostedService<IdentityProviderApplicationSeederHostedService>();

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
                options.SetTokenEndpointUris("/connect/token");
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);
                options.SetAccessTokenLifetime(identityProviderOptions.AccessTokenLifetime);
                options.SetRefreshTokenLifetime(identityProviderOptions.RefreshTokenLifetime);
                options.UseReferenceAccessTokens();
                options.UseReferenceRefreshTokens();
                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.EnableTokenEntryValidation();
            });

        serviceCollection.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        serviceCollection.AddAuthorization();

        return serviceCollection;
    }
}
