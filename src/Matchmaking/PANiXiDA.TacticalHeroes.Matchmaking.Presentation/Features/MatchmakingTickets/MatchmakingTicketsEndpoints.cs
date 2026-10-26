using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchmakingTickets;

internal sealed class MatchmakingTicketsEndpoints : IEndpointGroup
{
    public string Route { get; } = "matchmaking-tickets";
    public string Name { get; } = "MatchmakingTickets";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<MatchmakingTicketsEndpoints>(endpoints).RequireAuthorization();
    }
}
