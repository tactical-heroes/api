using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Application.Games.Events;

public sealed class GameCreationRequestedHandler(IGamesRepository gamesRepository, IEventBus eventBus)
    : IEventHandler<GameCreationRequested>
{
    public async Task HandleAsync(GameCreationRequested domainEvent, CancellationToken cancellationToken)
    {
        var game = Game.Create(
            mode: GameMode.Create(domainEvent.Mode).Value,
            firstPlayerId: PlayerId.Create(domainEvent.FirstPlayerId).Value,
            firstBuildId: BuildId.Create(domainEvent.FirstBuildId).Value,
            firstRating: Rating.Create(domainEvent.FirstRating),
            secondPlayerId: PlayerId.Create(domainEvent.SecondPlayerId).Value,
            secondBuildId: BuildId.Create(domainEvent.SecondBuildId).Value,
            secondRating: Rating.Create(domainEvent.SecondRating)).Value;

        gamesRepository.Add(game);

        await eventBus.PublishAsync(
            @event: new GameCreated(
                GameId: game.Id.Value,
                FirstMatchSearchId: domainEvent.FirstMatchSearchId,
                SecondMatchSearchId: domainEvent.SecondMatchSearchId),
            cancellationToken: cancellationToken);
    }
}
