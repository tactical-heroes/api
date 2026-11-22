using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Games.Presentation.Features.Games;

internal sealed class GamesEndpoints : IEndpointGroup
{
    public string Route { get; } = "games";
    public string Name { get; } = "Games";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<GamesEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
