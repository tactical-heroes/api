using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.Application;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        var messageStoreConnectionString =
            configuration.GetConnectionString(EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        hostBuilder.UseWolverineMediator<IdentityWriteDbContext>(
            messageStoreConnectionString,
            ApplicationAssembly.Instance,
            typeof(HostBuilderExtensions).Assembly);

        return hostBuilder;
    }
}
