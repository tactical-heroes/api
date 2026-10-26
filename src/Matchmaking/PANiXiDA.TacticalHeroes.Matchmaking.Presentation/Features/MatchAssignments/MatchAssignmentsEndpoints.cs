using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchAssignments;

internal sealed class MatchAssignmentsEndpoints : IEndpointGroup
{
    public string Route { get; } = "match-assignments";
    public string Name { get; } = "MatchAssignments";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<MatchAssignmentsEndpoints>(endpoints).RequireAuthorization();
    }
}
