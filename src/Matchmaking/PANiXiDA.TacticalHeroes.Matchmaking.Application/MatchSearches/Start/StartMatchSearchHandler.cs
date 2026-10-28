using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

public sealed class StartMatchSearchHandler(
    IMatchSearchesRepository matchSearchesRepository,
    IPlayersClient playersClient,
    IBuildsClient buildsClient,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    :
        ICommandHandler<StartMatchSearchCommand, Result>,
        IRequireAuthorization,
        IRequireTransaction
{
    public async Task<Result> HandleAsync(
        StartMatchSearchCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("A user account is required."));
        }

        var buildId = BuildId.Create(command.BuildId);
        var mode = MatchmakingMode.Create(command.Mode);
        var validation = Result.Combine(buildId, mode);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var playerResult = await playersClient.GetByUserIdAsync(userId, cancellationToken);
        if (playerResult.IsFailure)
        {
            return Result.Failure(playerResult.Errors);
        }

        var player = playerResult.Value;
        if (!await buildsClient.IsOwnedByPlayerAsync(
            buildId: command.BuildId,
            playerId: player.Id,
            cancellationToken: cancellationToken))
        {
            return Result.Failure(Error.NotFound("Build was not found."));
        }

        var playerId = PlayerId.Create(player.Id).Value;
        var existing = await matchSearchesRepository.GetActiveByPlayerIdForUpdateAsync(playerId, cancellationToken);
        var now = MatchmakingTimestamp.Create(timeProvider.GetUtcNow());
        if (existing is not null)
        {
            var expiration = existing.Expire(now);
            if (expiration.IsFailure)
            {
                return Result.Failure(Error.Conflict("Player is already searching or playing."));
            }

            matchSearchesRepository.Update(existing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var created = MatchSearch.Create(
            playerId: playerId,
            buildId: buildId.Value,
            rating: Raiting.Create(player.Rating),
            mode: mode.Value,
            queuedAt: now);
        if (created.IsFailure)
        {
            return Result.Failure(created.Errors);
        }

        matchSearchesRepository.Add(created.Value);

        return Result.Success();
    }
}
