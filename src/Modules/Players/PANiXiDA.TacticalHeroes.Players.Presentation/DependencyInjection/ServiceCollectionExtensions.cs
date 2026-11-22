using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Players.Contracts.Players;
using PANiXiDA.TacticalHeroes.Players.Presentation.Internal;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlayersApi, PlayersApi>();

        return serviceCollection;
    }
}
