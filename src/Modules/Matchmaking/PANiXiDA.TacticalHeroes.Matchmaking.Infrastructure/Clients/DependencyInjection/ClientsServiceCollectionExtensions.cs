using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Clients.DependencyInjection;

internal static class ClientsServiceCollectionExtensions
{
    public static IServiceCollection AddClients(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlayersClient, PlayersClient>();
        serviceCollection.AddScoped<IBuildsClient, BuildsClient>();

        return serviceCollection;
    }
}
