using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Tracking;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

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
                    $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "identity"))
                .UseSnakeCaseNamingConvention()
                .UseOpenIddict<Guid>();
        });

        return serviceCollection;
    }

    public static IServiceCollection AddReadPersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlReadEfRepository<IdentityReadDbContext>(configuration);
        serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
        serviceCollection.AddScoped<IRolesRepository, RolesRepository>();

        return serviceCollection;
    }
}
