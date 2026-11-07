using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;

public sealed class MatchSearchStateChangedHandler(
    IMatchSearchStatePublisher statePublisher,
    IMatchSearchesReadRepository matchSearchesReadRepository) : IEventHandler<MatchSearchStateChanged>
{
    public async Task HandleAsync(MatchSearchStateChanged domainEvent, CancellationToken cancellationToken)
    {
        var state = await matchSearchesReadRepository.GetStateByPlayerIdAsync(domainEvent.PlayerId, cancellationToken);
        if (state is not null)
        {
            await statePublisher.PublishAsync(
                playerId: domainEvent.PlayerId, state: state, cancellationToken: cancellationToken);
        }
    }
}
