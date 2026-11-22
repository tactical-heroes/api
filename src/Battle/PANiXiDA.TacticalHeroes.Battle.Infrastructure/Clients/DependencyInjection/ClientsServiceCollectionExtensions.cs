using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Clients.DependencyInjection;

public static class ClientsServiceCollectionExtensions
{
    public static IServiceCollection AddClients(this IServiceCollection services)
    {
        services.AddScoped<IBuildsClient, BuildsClient>();
        services.AddScoped<ICompendiumClient, CompendiumClient>();

        return services;
    }
}
