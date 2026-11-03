using FluentValidation.Results;

using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Application.Games.Events;

public sealed class GameCreationRequestedHandler(IGamesRepository gamesRepository)
    : IEventHandler<GameCreationRequested>
{
    public Task HandleAsync(GameCreationRequested domainEvent, CancellationToken cancellationToken)
    {
        var mode = GameMode.Create(domainEvent.Mode);
        var firstPlayerId = PlayerId.Create(domainEvent.FirstPlayerId);
        var firstBuildId = BuildId.Create(domainEvent.FirstBuildId);
        var secondPlayerId = PlayerId.Create(domainEvent.SecondPlayerId);
        var secondBuildId = BuildId.Create(domainEvent.SecondBuildId);
        var firstMatchSearchId = MatchSearchId.Create(domainEvent.FirstMatchSearchId);
        var secondMatchSearchId = MatchSearchId.Create(domainEvent.SecondMatchSearchId);

        var validation = Result.Combine(
            mode,
            firstPlayerId,
            firstBuildId,
            secondPlayerId,
            secondBuildId,
            firstMatchSearchId,
            secondMatchSearchId);

        if (validation.IsFailure)
        {
            throw new ValidationException(validation.Errors.Select(error => new ValidationFailure(
                propertyName: error.Metadata.GetValueOrDefault(Error.FieldMetadataKey) as string ?? string.Empty,
                errorMessage: error.Message)));
        }

        var created = Game.Create(
            mode: mode.Value,
            firstParticipant: GameParticipantSelection.Create(
                playerId: firstPlayerId.Value,
                buildId: firstBuildId.Value,
                rating: Rating.Create(domainEvent.FirstRating)),
            secondParticipant: GameParticipantSelection.Create(
                playerId: secondPlayerId.Value,
                buildId: secondBuildId.Value,
                rating: Rating.Create(domainEvent.SecondRating)),
            firstMatchSearchId: firstMatchSearchId.Value,
            secondMatchSearchId: secondMatchSearchId.Value);

        if (created.IsFailure)
        {
            throw new ValidationException(created.Errors.Select(error => new ValidationFailure(
                propertyName: error.Metadata.GetValueOrDefault(Error.FieldMetadataKey) as string ?? string.Empty,
                errorMessage: error.Message)));
        }

        var game = created.Value;
        gamesRepository.Add(game);

        return Task.CompletedTask;
    }
}
