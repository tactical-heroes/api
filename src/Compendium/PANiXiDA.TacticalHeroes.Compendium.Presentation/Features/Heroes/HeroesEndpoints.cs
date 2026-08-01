using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes;

internal sealed class HeroesEndpoints : IEndpointGroup
{
    internal const string IdRoute = "/{id:guid}";

    public string Route { get; } = "heroes";
    public string Name { get; } = "Heroes";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<HeroesEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
