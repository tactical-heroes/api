using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Api;

[Mapper]
internal static partial class PlayersApiMapper
{
    internal static partial PlayerProfileResponse ToResponse(PlayerProfileReadModel profile);
}
