using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();
    })
        .ConfigureServices((ctx, services) =>
        {
            services.AddPostgreSqlEfRepository<
                IdentityWriteDbContext, IdentityReadDbContext>(ctx.Configuration);
        })
    .RunMigrationsAsync<IdentityWriteDbContext>();
