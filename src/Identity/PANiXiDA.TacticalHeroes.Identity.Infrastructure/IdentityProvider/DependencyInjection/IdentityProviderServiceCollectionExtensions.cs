using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
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
        IConfiguration configuration)
    {
        var identityProviderOptions = configuration
            .GetSection(IdentityProviderOptions.SectionName)
            .Get<IdentityProviderOptions>() ?? new IdentityProviderOptions();

        serviceCollection.Configure<IdentityProviderOptions>(
            configuration.GetSection(IdentityProviderOptions.SectionName));

        serviceCollection.AddScoped<IUserCredentialsService, UserCredentialsService>();
        serviceCollection.AddScoped<IdentityProviderApplicationSeeder>();
        serviceCollection.AddHostedService<IdentityProviderApplicationSeederHostedService>();
        serviceCollection.AddDataProtection()
            .PersistKeysToDbContext<IdentityWriteDbContext>();

        serviceCollection
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = identityProviderOptions.User.RequireUniqueEmail;
                options.Password.RequiredLength = identityProviderOptions.Password.RequiredLength;
                options.Password.RequiredUniqueChars = identityProviderOptions.Password.RequiredUniqueChars;
                options.Password.RequireDigit = identityProviderOptions.Password.RequireDigit;
                options.Password.RequireLowercase = identityProviderOptions.Password.RequireLowercase;
                options.Password.RequireNonAlphanumeric = identityProviderOptions.Password.RequireNonAlphanumeric;
                options.Password.RequireUppercase = identityProviderOptions.Password.RequireUppercase;
                options.Tokens.EmailConfirmationTokenProvider = identityProviderOptions.TokenProviders.EmailConfirmation;
                options.Tokens.PasswordResetTokenProvider = identityProviderOptions.TokenProviders.PasswordReset;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityWriteDbContext>()
            .AddTokenProvider<EmailConfirmationTokenProvider>(identityProviderOptions.TokenProviders.EmailConfirmation)
            .AddTokenProvider<PasswordResetTokenProvider>(identityProviderOptions.TokenProviders.PasswordReset);

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
