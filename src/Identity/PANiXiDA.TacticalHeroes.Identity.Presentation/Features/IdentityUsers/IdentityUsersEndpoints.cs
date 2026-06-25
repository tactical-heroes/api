using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers;

internal sealed class IdentityUsersEndpoints : IEndpointGroup
{
    public string Route { get; } = "identity/auth";
    public string Name { get; } = "Identity users";
    public ApiVersion ApiVersion { get; } = new(1, 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<IdentityUsersEndpoints>(endpoints);
    }
}
