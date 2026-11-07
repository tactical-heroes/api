using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public sealed class MatchSearchHub(IMediator mediator) : Hub<IMatchSearchClient>
{
    public const string Route = "/api/v1/match-searches/state";

    public static HubEndpointConventionBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapHub<MatchSearchHub>(Route).RequireAuthorization();
    }

    public override async Task OnConnectedAsync()
    {
        var result = await mediator.SendAsync(
            WatchMatchSearchMapper.ToCommand(Context.ConnectionId), Context.ConnectionAborted);
        if (result.IsFailure)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    internal static string PlayerGroup(Guid playerId) => $"player:{playerId:N}";
}
