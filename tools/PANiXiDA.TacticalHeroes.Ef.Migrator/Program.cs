using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString(
            EfConstants.PostgreSqlConnectionStringName) ??
            throw new InvalidOperationException(
                $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        services.AddDbContext<IdentityWriteDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "identity"))
                .UseSnakeCaseNamingConvention()
                .UseOpenIddict<Guid>();
        });

        services.AddPostgreSqlWriteEfRepository<CompendiumWriteDbContext>(
            context.Configuration);
    })
    .Build();

await host.RunMigrationsAsync<IdentityWriteDbContext>();
await host.RunMigrationsAsync<CompendiumWriteDbContext>();
