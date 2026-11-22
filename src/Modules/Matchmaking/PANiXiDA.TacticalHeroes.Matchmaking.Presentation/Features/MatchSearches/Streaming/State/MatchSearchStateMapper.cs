using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

[Mapper]
internal static partial class MatchSearchStateMapper
{
    internal static partial MatchSearchStateResponse ToResponse(MatchSearchReadModel readModel);
}
