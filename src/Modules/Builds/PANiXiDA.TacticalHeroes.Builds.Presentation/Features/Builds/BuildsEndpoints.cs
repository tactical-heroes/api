using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Builds.Presentation.Features.Builds;

internal sealed class BuildsEndpoints : IEndpointGroup
{
    public string Route { get; } = "builds";
    public string Name { get; } = "Builds";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<BuildsEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
