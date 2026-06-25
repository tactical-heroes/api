using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddHttp(configuration);

        return serviceCollection;
    }
}
