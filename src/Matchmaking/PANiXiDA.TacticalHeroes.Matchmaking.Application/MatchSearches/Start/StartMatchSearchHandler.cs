using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;
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
        ICommandHandler<StartMatchSearchCommand, Result<StartMatchSearchResult>>,
        IRequireAuthorization,
        IRequireTransaction
{
    public async Task<Result<StartMatchSearchResult>> HandleAsync(
        StartMatchSearchCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Result.Failure<StartMatchSearchResult>(Error.Unauthorized("A user account is required."));
        }

        var buildId = BuildId.Create(command.BuildId);
        var mode = MatchmakingMode.Create(command.Mode);
        var validation = Result.Combine(buildId, mode);
        if (validation.IsFailure)
        {
            return Result.Failure<StartMatchSearchResult>(validation.Errors);
        }

        var player = await playersClient.GetByUserIdAsync(userId, cancellationToken);
        if (player is null)
        {
            return Result.Failure<StartMatchSearchResult>(Error.NotFound("Player was not found."));
        }

        if (!await buildsClient.IsOwnedByPlayerAsync(
            buildId: command.BuildId,
            playerId: player.Id,
            cancellationToken: cancellationToken))
        {
            return Result.Failure<StartMatchSearchResult>(Error.NotFound("Build was not found."));
        }

        var playerId = PlayerId.Create(player.Id).Value;
        var existing = await matchSearchesRepository.GetActiveByPlayerIdForUpdateAsync(playerId, cancellationToken);
        var now = MatchmakingTimestamp.Create(timeProvider.GetUtcNow());
        if (existing is not null)
        {
            var expiration = existing.Expire(now);
            if (expiration.IsFailure)
            {
                return Result.Failure<StartMatchSearchResult>(
                    Error.Conflict("Player is already searching or playing."));
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
            return Result.Failure<StartMatchSearchResult>(created.Errors);
        }

        var search = created.Value;
        matchSearchesRepository.Add(search);

        return Result.Success(new StartMatchSearchResult(
            Id: search.Id.Value,
            Status: search.Status.Name,
            QueuedAt: search.QueuedAt.Value,
            ExpiresAt: search.ExpiresAt.Value));
    }
}
