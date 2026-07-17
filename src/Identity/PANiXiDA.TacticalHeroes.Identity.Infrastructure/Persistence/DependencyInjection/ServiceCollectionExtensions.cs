using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Tracking;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWritePersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.TryAddScoped<IAggregateTracker, AggregateTracker>();
        serviceCollection.TryAddScoped<IUnitOfWork, EfUnitOfWork<IdentityWriteDbContext>>();

        serviceCollection.AddDbContext<IdentityWriteDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(
                EfConstants.PostgreSqlConnectionStringName) ??
                throw new InvalidOperationException(
                    message: $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "identity"))
                .UseSnakeCaseNamingConvention()
                .UseOpenIddict<Guid>();
        });
        serviceCollection.AddScoped<IUsersWriteRepository, UsersWriteRepository>();
        serviceCollection.AddScoped<IRolesWriteRepository, RolesRepository>();

        return serviceCollection;
    }

    public static IServiceCollection AddReadPersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlReadEfRepository<IdentityReadDbContext>(configuration);
        serviceCollection.AddDbContext<IdentityReadDbContext>((provider, options) =>
            options.AddInterceptors(provider.GetServices<IInterceptor>()));
        serviceCollection.AddScoped<IUsersReadRepository, UsersReadRepository>();
        serviceCollection.AddScoped<IRolesReadRepository, RolesReadRepository>();
        serviceCollection.AddScoped<IOAuthUsersRepository, OAuthUsersRepository>();
        serviceCollection.AddScoped<IOAuthClientsRepository, OAuthClientsRepository>();

        return serviceCollection;
    }
}
