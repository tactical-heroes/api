using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Organization.Product.Module.Infrastructure.Persistence.Core;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();
    })
        .ConfigureServices((ctx, services) =>
        {
            services.AddPostgreSqlEfRepository<
                TemplateWriteDbContext, TemplateReadDbContext>(ctx.Configuration);
        })
    .RunMigrationsAsync<TemplateWriteDbContext>();
