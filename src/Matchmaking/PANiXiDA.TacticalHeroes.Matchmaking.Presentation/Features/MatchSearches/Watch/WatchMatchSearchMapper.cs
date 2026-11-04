using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

[Mapper]
internal static partial class WatchMatchSearchMapper
{
    internal static WatchMatchSearchCommand ToCommand(string connectionId) => new(connectionId);

    internal static partial MatchSearchStateResponse ToResponse(MatchSearchReadModel readModel);
}
