using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Organization.Product.Module.Application;
using Organization.Product.Module.Infrastructure.Persistence.Core;

namespace Organization.Product.Module.Infrastructure.DependencyInjection;

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

        hostBuilder.UseWolverineMediator<TemplateWriteDbContext>(
            messageStoreConnectionString,
            ApplicationAssembly.Instance);

        return hostBuilder;
    }
}
