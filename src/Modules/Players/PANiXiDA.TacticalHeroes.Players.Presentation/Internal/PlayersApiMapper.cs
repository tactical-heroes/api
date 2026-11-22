using PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Internal;

[Mapper]
internal static partial class PlayersApiMapper
{
    internal static partial GetInternalPlayerQuery ToQuery(Guid userId);
    internal static partial PlayerResponse ToResponse(PlayerReadModel player);
}
