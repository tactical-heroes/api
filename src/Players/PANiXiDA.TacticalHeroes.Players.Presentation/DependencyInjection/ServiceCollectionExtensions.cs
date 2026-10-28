using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;
using PANiXiDA.TacticalHeroes.Players.Presentation.Api;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlayerProfiles, PlayerProfiles>();
        serviceCollection.AddScoped<IPlayersApi, PlayersApi>();

        return serviceCollection;
    }
}
