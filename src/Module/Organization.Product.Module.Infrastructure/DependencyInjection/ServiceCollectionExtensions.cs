using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Organization.Product.Module.Infrastructure.Persistence.Core;

namespace Organization.Product.Module.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            TemplateWriteDbContext, TemplateReadDbContext>(configuration);

        serviceCollection.AddWolverineMediator<TemplateWriteDbContext>();

        return serviceCollection;
    }
}
