using Microsoft.Extensions.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Notifications.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }
}
