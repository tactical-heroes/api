using PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.GetInternalPlayer;
using PANiXiDA.TacticalHeroes.Modules.Players.Contracts.Players;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Presentation.Internal;

[Mapper]
internal static partial class PlayersApiMapper
{
    internal static partial GetInternalPlayerQuery ToQuery(Guid userId);
    internal static partial PlayerResponse ToResponse(PlayerReadModel player);
}
