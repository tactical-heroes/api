namespace PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;

public interface IPlayersRepository : IRepository<PlayerId, Player>
{
    Task AddIfMissingAsync(Player player, CancellationToken cancellationToken);
}
