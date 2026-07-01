using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Tracking;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.TryAddSingleton(TimeProvider.System);
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

        serviceCollection.AddPostgreSqlReadEfRepository<IdentityReadDbContext>(configuration);
        serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
        serviceCollection.AddScoped<IRolesRepository, RolesRepository>();

        serviceCollection.AddScheduling(configuration);
        serviceCollection.AddIdentityProvider(configuration);
        serviceCollection.AddMessaging(configuration);

        serviceCollection.AddWolverineMediator<IdentityWriteDbContext>();

        return serviceCollection;
    }
}
