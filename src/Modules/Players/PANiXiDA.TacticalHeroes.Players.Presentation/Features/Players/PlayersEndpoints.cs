using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Features.Players;

internal sealed class PlayersEndpoints : IEndpointGroup
{
    public string Route { get; } = "players";
    public string Name { get; } = "Players";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<PlayersEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
