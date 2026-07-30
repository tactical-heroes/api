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
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWritePersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.TryAddScoped<IAggregateTracker, AggregateTracker>();
        serviceCollection.TryAddKeyedScoped<IUnitOfWork, EfUnitOfWork<IdentityWriteDbContext>>(
            serviceKey: typeof(IdentityWriteDbContext));

        serviceCollection.AddDbContext<IdentityWriteDbContext>(optionsAction: options =>
        {
            var connectionString = configuration.GetConnectionString(
                name: EfConstants.PostgreSqlConnectionStringName) ??
                throw new InvalidOperationException(
                    message: $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

            options
                .UseNpgsql(connectionString: connectionString, npgsqlOptionsAction: npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(tableName: "__ef_migrations_history", schema: "identity"))
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
        serviceCollection.AddPostgreSqlReadEfRepository<IdentityReadDbContext>(configuration: configuration);
        serviceCollection.AddDbContext<IdentityReadDbContext>(optionsAction: (provider, options) =>
            options.AddInterceptors(interceptors: provider.GetServices<IInterceptor>()));
        serviceCollection.AddScoped<IOAuthUsersRepository, OAuthUsersRepository>();
        serviceCollection.AddScoped<IOAuthClientsRepository, OAuthClientsRepository>();

        return serviceCollection;
    }
}
