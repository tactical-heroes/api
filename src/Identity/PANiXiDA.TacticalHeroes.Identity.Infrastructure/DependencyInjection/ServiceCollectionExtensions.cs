using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenIddict.Validation.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

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

        serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
        serviceCollection.AddScoped<IRolesRepository, RolesRepository>();
        serviceCollection.AddScoped<IPasswordHashingService, PasswordHashingService>();
        serviceCollection.AddScoped<IUserTokenService, UserTokenService>();
        serviceCollection.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
        serviceCollection.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
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
