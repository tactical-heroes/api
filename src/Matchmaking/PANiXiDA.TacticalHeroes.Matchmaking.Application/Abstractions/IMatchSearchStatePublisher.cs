namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IMatchSearchStatePublisher
{
    Task PublishAsync(Guid playerId, CancellationToken cancellationToken);
}
