using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Permissions;

internal sealed class PermissionsEndpoints : IEndpointGroup
{
    public string Route { get; } = "permissions";
    public string Name { get; } = "Permissions";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<PermissionsEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
