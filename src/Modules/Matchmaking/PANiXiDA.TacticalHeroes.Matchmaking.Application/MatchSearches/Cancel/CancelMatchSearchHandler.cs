using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Cancel;

public sealed class CancelMatchSearchHandler(
    IMatchSearchesRepository matchSearchesRepository,
    IPlayersClient playersClient,
    ICurrentUser currentUser)
    : ICommandHandler<CancelMatchSearchCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(CancelMatchSearchCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("A user account is required."));
        }

        var searchId = MatchSearchId.Create(command.Id);

        if (searchId.IsFailure)
        {
            return Result.Failure(searchId.Errors);
        }

        var player = await playersClient.GetByUserIdAsync(userId, cancellationToken);

        if (player is null)
        {
            return Result.Failure(Error.NotFound("Player was not found."));
        }

        var playerId = PlayerId.Create(player.Id);

        if (playerId.IsFailure)
        {
            return Result.Failure(playerId.Errors);
        }

        var search = await matchSearchesRepository.GetByIdAsync(searchId.Value, cancellationToken);

        if (search is null || search.PlayerId != playerId.Value)
        {
            return Result.Failure(Error.NotFound("Match search was not found."));
        }

        if (search.Status == MatchSearchStatus.Cancelled)
        {
            return Result.Success();
        }

        var cancelled = search.Cancel();

        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        matchSearchesRepository.Update(search);

        return Result.Success();
    }
}
