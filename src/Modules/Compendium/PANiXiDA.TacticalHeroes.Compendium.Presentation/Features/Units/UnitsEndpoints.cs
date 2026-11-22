using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units;

internal sealed class UnitsEndpoints : IEndpointGroup
{
    internal const string IdRoute = "/{id:guid}";

    public string Route { get; } = "units";
    public string Name { get; } = "Units";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<UnitsEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
