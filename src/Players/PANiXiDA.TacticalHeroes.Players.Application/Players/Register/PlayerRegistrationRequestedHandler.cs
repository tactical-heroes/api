using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Events;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Register;

public sealed class PlayerRegistrationRequestedHandler(IPlayersRepository playersRepository)
    : IEventHandler<PlayerRegistrationRequested>
{
    public Task HandleAsync(
        PlayerRegistrationRequested domainEvent,
        CancellationToken cancellationToken)
    {
        var userId = UserId.Create(domainEvent.UserId).Value;
        var player = Player.Create(userId);

        return playersRepository.AddIfMissingAsync(player, cancellationToken);
    }
}
