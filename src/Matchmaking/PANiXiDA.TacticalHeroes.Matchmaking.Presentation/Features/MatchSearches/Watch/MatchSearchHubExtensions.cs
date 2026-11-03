using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public static class MatchSearchHubExtensions
{
    public const string Route = "/api/v1/match-searches/state";

    public static HubEndpointConventionBuilder MapMatchSearchHub(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapHub<MatchSearchHub>(Route).RequireAuthorization();
    }
}
