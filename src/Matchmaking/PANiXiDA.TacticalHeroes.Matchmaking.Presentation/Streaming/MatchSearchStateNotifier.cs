using Microsoft.AspNetCore.SignalR;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

internal sealed class MatchSearchStateNotifier(
    IHubContext<MatchSearchHub, IMatchSearchClient> hubContext) : IMatchSearchStateNotifier
{
    public Task SendAsync(string connectionId, MatchSearchReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Client(connectionId).StateChangedAsync(
            WatchMatchSearchMapper.ToResponse(state), cancellationToken);
    }

    public Task PublishAsync(Guid playerId, MatchSearchReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(MatchSearchHub.PlayerGroup(playerId)).StateChangedAsync(
            WatchMatchSearchMapper.ToResponse(state), cancellationToken);
    }

    public Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken)
    {
        return hubContext.Groups.AddToGroupAsync(
            connectionId: connectionId,
            groupName: MatchSearchHub.PlayerGroup(playerId),
            cancellationToken: cancellationToken);
    }
}
