using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Watch;

internal static class WatchMatchSearchMapper
{
    internal static WatchMatchSearchCommand ToCommand(string connectionId) => new(connectionId);
}
