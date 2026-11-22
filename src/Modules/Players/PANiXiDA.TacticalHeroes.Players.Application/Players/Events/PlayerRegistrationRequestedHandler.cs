using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Events;
using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.Events;

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
