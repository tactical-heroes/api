using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Players.Contracts.Players;
using PANiXiDA.TacticalHeroes.Modules.Players.Presentation.Internal;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlayersApi, PlayersApi>();

        return serviceCollection;
    }
}
