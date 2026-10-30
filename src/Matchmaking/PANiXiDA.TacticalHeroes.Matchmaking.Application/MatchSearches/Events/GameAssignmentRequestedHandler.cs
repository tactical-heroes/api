using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;

public sealed class GameAssignmentRequestedHandler(IMatchSearchesRepository matchSearchesRepository)
    : IEventHandler<GameAssignmentRequested>
{
    public async Task HandleAsync(GameAssignmentRequested domainEvent, CancellationToken cancellationToken)
    {
        var gameId = GameId.Create(domainEvent.GameId).Value;
        var firstId = MatchSearchId.Create(domainEvent.FirstMatchSearchId).Value;
        var secondId = MatchSearchId.Create(domainEvent.SecondMatchSearchId).Value;

        if (firstId == secondId)
        {
            throw new InvalidOperationException("A game must be assigned to two different match searches.");
        }

        var first = await matchSearchesRepository.GetByIdAsync(firstId, cancellationToken);
        var second = await matchSearchesRepository.GetByIdAsync(secondId, cancellationToken);

        if (first is null || second is null)
        {
            throw new InvalidOperationException("Both match searches must exist before assigning a game.");
        }

        var firstNeedsAssignment = first.GameId != gameId;
        var secondNeedsAssignment = second.GameId != gameId;
        var assigned = Result.Combine(first.AssignGame(gameId), second.AssignGame(gameId));

        if (assigned.IsFailure)
        {
            throw new InvalidOperationException("The game could not be assigned to both match searches.");
        }

        if (firstNeedsAssignment)
        {
            matchSearchesRepository.Update(first);
        }

        if (secondNeedsAssignment)
        {
            matchSearchesRepository.Update(second);
        }
    }
}
