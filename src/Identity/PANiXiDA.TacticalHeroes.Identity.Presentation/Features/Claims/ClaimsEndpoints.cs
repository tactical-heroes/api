using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims;

internal sealed class ClaimsEndpoints : IEndpointGroup
{
    public string Route { get; } = "claims";
    public string Name { get; } = "Claims";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<ClaimsEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
