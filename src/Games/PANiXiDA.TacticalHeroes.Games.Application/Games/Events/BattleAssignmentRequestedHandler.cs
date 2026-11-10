using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

namespace PANiXiDA.TacticalHeroes.Games.Application.Games.Events;

public sealed class BattleAssignmentRequestedHandler(IGamesRepository gamesRepository)
    : IEventHandler<BattleAssignmentRequested>
{
    public async Task HandleAsync(BattleAssignmentRequested domainEvent, CancellationToken cancellationToken)
    {
        var gameId = GameId.Create(domainEvent.GameId).Value;
        var battleId = BattleId.Create(domainEvent.BattleId).Value;
        var game = await gamesRepository.GetByIdAsync(gameId, cancellationToken)
            ?? throw new InvalidOperationException("The game must exist before assigning its battle.");
        if (game.BattleId == battleId)
        {
            return;
        }

        var assigned = game.AssignBattle(battleId);
        if (assigned.IsFailure)
        {
            throw new InvalidOperationException("The battle could not be assigned to the game.");
        }

        gamesRepository.Update(game);
    }
}
