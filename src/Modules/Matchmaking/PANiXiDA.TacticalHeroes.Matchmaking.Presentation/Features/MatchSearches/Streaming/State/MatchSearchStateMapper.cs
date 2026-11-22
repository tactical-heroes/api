using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

[Mapper]
internal static partial class MatchSearchStateMapper
{
    internal static partial MatchSearchStateResponse ToResponse(MatchSearchReadModel readModel);
}
