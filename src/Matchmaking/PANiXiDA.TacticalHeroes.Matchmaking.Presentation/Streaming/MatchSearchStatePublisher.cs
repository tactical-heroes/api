using Microsoft.AspNetCore.SignalR;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

internal sealed class MatchSearchStatePublisher(
    IHubContext<MatchSearchHub, IMatchSearchClient> hubContext) : IMatchSearchStatePublisher
{
    public Task PublishAsync(Guid playerId, MatchSearchStateReadModel state, CancellationToken cancellationToken)
    {
        return hubContext.Clients.Group(MatchSearchHub.PlayerGroup(playerId)).StateChangedAsync(
            WatchMatchSearchMapper.ToResponse(state), cancellationToken);
    }
}
