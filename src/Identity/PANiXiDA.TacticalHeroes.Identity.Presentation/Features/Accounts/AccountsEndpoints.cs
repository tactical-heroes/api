using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts;

internal sealed class AccountsEndpoints : IEndpointGroup
{
    public string Route { get; } = "accounts";
    public string Name { get; } = "Accounts";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<AccountsEndpoints>(endpoints);
    }
}
