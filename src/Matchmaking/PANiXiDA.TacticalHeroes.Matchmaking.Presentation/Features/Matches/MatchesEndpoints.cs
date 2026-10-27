using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.Matches;

internal sealed class MatchesEndpoints : IEndpointGroup
{
    public string Route { get; } = "matches";
    public string Name { get; } = "Matches";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<MatchesEndpoints>(endpoints).RequireAuthorization();
    }
}
