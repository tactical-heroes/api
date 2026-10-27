namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;

public interface IPlayersClient
{
    Task<MatchmakingPlayer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
