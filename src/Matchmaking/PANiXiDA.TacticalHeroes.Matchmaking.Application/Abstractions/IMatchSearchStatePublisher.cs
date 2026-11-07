using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IMatchSearchStatePublisher
{
    Task PublishAsync(Guid playerId, MatchSearchReadModel state, CancellationToken cancellationToken);
}
