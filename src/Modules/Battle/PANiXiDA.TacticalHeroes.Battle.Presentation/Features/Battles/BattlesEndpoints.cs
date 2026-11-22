using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Battle.Presentation.Features.Battles;

internal sealed class BattlesEndpoints : IEndpointGroup
{
    public string Route { get; } = "battles";
    public string Name { get; } = "Battles";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<BattlesEndpoints>(endpoints).RequireAuthorization();
    }
}
