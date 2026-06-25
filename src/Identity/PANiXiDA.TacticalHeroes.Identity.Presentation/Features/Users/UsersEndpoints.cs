using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users;

internal sealed class UsersEndpoints : IEndpointGroup
{
    public string Route { get; } = "identity/auth";
    public string Name { get; } = "Users";
    public ApiVersion ApiVersion { get; } = new(1, 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<UsersEndpoints>(endpoints);
    }
}
