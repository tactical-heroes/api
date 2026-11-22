using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches;

internal sealed class MatchSearchesEndpoints : IEndpointGroup
{
    public string Route { get; } = "match-searches";
    public string Name { get; } = "MatchSearches";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper.MapGroupEndpoints<MatchSearchesEndpoints>(endpoints).RequireAuthorization();
    }
}
