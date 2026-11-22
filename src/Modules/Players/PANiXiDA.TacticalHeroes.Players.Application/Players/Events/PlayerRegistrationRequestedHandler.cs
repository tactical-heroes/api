using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Events;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Events;

public sealed class PlayerRegistrationRequestedHandler(IPlayersRepository playersRepository)
    : IEventHandler<PlayerRegistrationRequested>
{
    public async Task HandleAsync(
        PlayerRegistrationRequested domainEvent,
        CancellationToken cancellationToken)
    {
        var userId = UserId.Create(domainEvent.UserId).Value;
        var existingPlayer = await playersRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existingPlayer is not null)
        {
            return;
        }

        var player = Player.Create(userId);
        playersRepository.Add(player);
    }
}
