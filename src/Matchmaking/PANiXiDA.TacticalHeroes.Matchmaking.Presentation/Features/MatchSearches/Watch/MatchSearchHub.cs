using System.Globalization;

using Asp.Versioning;

using Microsoft.AspNetCore.SignalR;

using PANiXiDA.Core.Presentation.SignalR.Hubs;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public sealed class MatchSearchHub(IMediator mediator) : Hub<IMatchSearchClient>, IDocumentedHubEndpoint
{
    public static string Name => "matchmaking";

    public static string Summary => "Matchmaking SignalR";

    public static ApiVersion ApiVersion { get; } = new(1, 0);

    public static string Route { get; } =
        string.Create(CultureInfo.InvariantCulture, $"/api/v{ApiVersion.MajorVersion}/hubs/match-searches");

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
