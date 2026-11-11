using Asp.Versioning;

using Microsoft.AspNetCore.SignalR;

using PANiXiDA.Core.Presentation.SignalR.Hubs;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public sealed class MatchSearchHub(IMediator mediator) : Hub<IMatchSearchClient>, IDocumentedHubEndpoint
{
    public const string Route = "/api/v1/match-searches/state";

    public static string Name => "matchmaking";

    public static string Summary => "Matchmaking SignalR";

    public static ApiVersion ApiVersion => new(1, 0);

    public static HubEndpointConventionBuilder Map(HubMapBuilder hubs)
    {
        return hubs.MapHub<MatchSearchHub>(Route).RequireAuthorization();
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
