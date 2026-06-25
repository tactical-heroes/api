using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace Organization.Product.Module.Presentation.Features.Users;

internal sealed class UsersEndpoints : IEndpointGroup
{
    internal const string IdRoute = "{id:guid}";

    public string Route { get; } = "users";
    public string Name { get; } = "Users";
    public ApiVersion ApiVersion { get; } = new(1, 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<UsersEndpoints>(endpoints);
    }
}
