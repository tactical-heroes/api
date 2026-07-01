using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

internal static class IdentityProviderServiceCollectionExtensions
{
    private const int MinimumPasswordLength = 8;

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
        serviceCollection.Configure<EmailConfirmationTokenProviderOptions>(options =>
        {
            options.Name = IdentityTokenProviderNames.EmailConfirmation;
            options.TokenLifespan = identityProviderOptions.EmailConfirmationTokenLifetime;
        });
        serviceCollection.Configure<PasswordResetTokenProviderOptions>(options =>
        {
            options.Name = IdentityTokenProviderNames.PasswordReset;
            options.TokenLifespan = identityProviderOptions.PasswordResetTokenLifetime;
        });

        serviceCollection
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = MinimumPasswordLength;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Tokens.EmailConfirmationTokenProvider = IdentityTokenProviderNames.EmailConfirmation;
                options.Tokens.PasswordResetTokenProvider = IdentityTokenProviderNames.PasswordReset;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityWriteDbContext>()
            .AddTokenProvider<EmailConfirmationTokenProvider>(IdentityTokenProviderNames.EmailConfirmation)
            .AddTokenProvider<PasswordResetTokenProvider>(IdentityTokenProviderNames.PasswordReset);

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
