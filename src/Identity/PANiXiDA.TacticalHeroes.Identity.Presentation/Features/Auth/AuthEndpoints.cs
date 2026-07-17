using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth;

internal sealed class AuthEndpoints : IEndpointGroup
{
    public string Route { get; } = "auth";
    public string Name { get; } = "Auth";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<AuthEndpoints>(endpoints);
    }
}
