using FluentValidation.Results;

using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Application.Games.Events;

public sealed class GameCreationRequestedHandler(IGamesRepository gamesRepository, IEventBus eventBus)
    : IEventHandler<GameCreationRequested>
{
    public async Task HandleAsync(GameCreationRequested domainEvent, CancellationToken cancellationToken)
    {
        var mode = GameMode.Create(domainEvent.Mode);
        var firstPlayerId = PlayerId.Create(domainEvent.FirstPlayerId);
        var firstBuildId = BuildId.Create(domainEvent.FirstBuildId);
        var secondPlayerId = PlayerId.Create(domainEvent.SecondPlayerId);
        var secondBuildId = BuildId.Create(domainEvent.SecondBuildId);
        var firstMatchSearchId = MatchSearchId.Create(domainEvent.FirstMatchSearchId);
        var secondMatchSearchId = MatchSearchId.Create(domainEvent.SecondMatchSearchId);

        ThrowIfInvalid(Result.Combine(
            mode,
            firstPlayerId,
            firstBuildId,
            secondPlayerId,
            secondBuildId,
            firstMatchSearchId,
            secondMatchSearchId));

        var created = Game.Create(
            mode: mode.Value,
            firstPlayerId: firstPlayerId.Value,
            firstBuildId: firstBuildId.Value,
            firstRating: Rating.Create(domainEvent.FirstRating),
            secondPlayerId: secondPlayerId.Value,
            secondBuildId: secondBuildId.Value,
            secondRating: Rating.Create(domainEvent.SecondRating),
            firstMatchSearchId: firstMatchSearchId.Value,
            secondMatchSearchId: secondMatchSearchId.Value);

        ThrowIfInvalid(created);

        var game = created.Value;
        gamesRepository.Add(game);

        foreach (var domainEventToPublish in game.GetDomainEvents())
        {
            await eventBus.PublishAsync(domainEventToPublish, cancellationToken);
        }

        game.ClearDomainEvents();
    }

    private static void ThrowIfInvalid(Result result)
    {
        if (result.IsFailure)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(
                propertyName: error.Metadata.GetValueOrDefault(Error.FieldMetadataKey) as string ?? string.Empty,
                errorMessage: error.Message)));
        }
    }
}
