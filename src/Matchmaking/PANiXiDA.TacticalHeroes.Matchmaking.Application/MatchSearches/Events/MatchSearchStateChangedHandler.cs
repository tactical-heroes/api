using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;

public sealed class MatchSearchStateChangedHandler(IMatchSearchStatePublisher statePublisher)
    : IEventHandler<MatchSearchStateChanged>
{
    public Task HandleAsync(MatchSearchStateChanged domainEvent, CancellationToken cancellationToken)
    {
        return statePublisher.PublishAsync(domainEvent.PlayerId, cancellationToken);
    }
}
