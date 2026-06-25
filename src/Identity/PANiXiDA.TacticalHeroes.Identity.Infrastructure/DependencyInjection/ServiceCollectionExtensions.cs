using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityRoles.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityUsers.Write;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            IdentityWriteDbContext, IdentityReadDbContext>(configuration);

        serviceCollection.AddIdentityServices();
        serviceCollection.AddOpenIddictServices();

        serviceCollection.AddWolverineMediator<IdentityWriteDbContext>();

        return serviceCollection;
    }

    private static void AddIdentityServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton(TimeProvider.System);

        serviceCollection.AddScoped<IIdentityUsersRepository, IdentityUsersRepository>();
        serviceCollection.AddScoped<IIdentityRolesRepository, IdentityRolesRepository>();
        serviceCollection.AddScoped<IPasswordHashingService, PasswordHashingService>();
        serviceCollection.AddScoped<IIdentityTokenService, IdentityTokenService>();
        serviceCollection.AddScoped<IIdentityClaimsProvider, IdentityClaimsProvider>();
        serviceCollection.AddScoped<IIdentityAuthenticationService, IdentityAuthenticationService>();
    }

    private static void AddOpenIddictServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddQuartz();
        serviceCollection.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

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
                options.AcceptAnonymousClients();
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
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
    }
}
