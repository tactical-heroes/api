using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Api;

internal sealed class PlayersApi(IPlayerProfiles playerProfiles) : IPlayersApi
{
    public async Task<PlayerProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await playerProfiles.GetByUserIdAsync(userId, cancellationToken);

        return profile is null ? null : PlayersApiMapper.ToResponse(profile);
    }
}
