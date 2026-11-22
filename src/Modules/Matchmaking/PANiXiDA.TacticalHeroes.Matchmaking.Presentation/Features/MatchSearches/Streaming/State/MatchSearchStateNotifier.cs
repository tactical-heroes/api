using Microsoft.AspNetCore.SignalR;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Write.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

internal sealed class MatchSearchStateNotifier(
    IHubContext<MatchSearchHub, IMatchSearchClient> hubContext) : IMatchSearchStateNotifier
{
    public Task SendAsync(string connectionId, MatchSearchReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Client(connectionId).StateChangedAsync(
            MatchSearchStateMapper.ToResponse(state), cancellationToken);
    }

    public Task PublishAsync(Guid playerId, MatchSearchReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(MatchSearchHub.PlayerGroup(playerId)).StateChangedAsync(
            MatchSearchStateMapper.ToResponse(state), cancellationToken);
    }

    public Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken)
    {
        return hubContext.Groups.AddToGroupAsync(
            connectionId: connectionId,
            groupName: MatchSearchHub.PlayerGroup(playerId),
            cancellationToken: cancellationToken);
    }
}
