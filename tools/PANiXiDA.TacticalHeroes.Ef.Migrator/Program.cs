using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();
    })
        .ConfigureServices((ctx, services) =>
        {
            var connectionString = ctx.Configuration.GetConnectionString(
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
        })
    .RunMigrationsAsync<IdentityWriteDbContext>();
