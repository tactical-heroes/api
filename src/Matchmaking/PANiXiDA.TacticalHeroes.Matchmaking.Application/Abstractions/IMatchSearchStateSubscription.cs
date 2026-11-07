using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IMatchSearchStateSubscription
{
    Task SendAsync(string connectionId, MatchSearchReadModel state, CancellationToken cancellationToken);
    Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken);
}
