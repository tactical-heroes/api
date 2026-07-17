using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management;

internal sealed class AccountManagementEndpoints : IEndpointGroup
{
    internal const string IdRoute = "/{id:guid}";

    public string Route { get; } = "accounts";
    public string Name { get; } = "Accounts";
    public ApiVersion ApiVersion { get; } = new(1, 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<AccountManagementEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
