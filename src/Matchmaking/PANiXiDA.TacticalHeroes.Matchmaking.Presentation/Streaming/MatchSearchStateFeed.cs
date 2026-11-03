using Microsoft.AspNetCore.SignalR;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

internal sealed class MatchSearchStateFeed(IHubContext<MatchSearchHub> hubContext) : IMatchSearchStateFeed
{
    public Task SendAsync(string connectionId, MatchSearchStateReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Client(connectionId).SendAsync(
            method: MatchSearchHub.StateChangedMethod, arg1: state, cancellationToken: cancellationToken);
    }

    public Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken)
    {
        return hubContext.Groups.AddToGroupAsync(
            connectionId: connectionId,
            groupName: MatchSearchHub.PlayerGroup(playerId),
            cancellationToken: cancellationToken);
    }
}
