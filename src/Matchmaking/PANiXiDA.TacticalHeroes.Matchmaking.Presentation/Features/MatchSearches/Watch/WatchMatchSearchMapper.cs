using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

internal static class WatchMatchSearchMapper
{
    internal static WatchMatchSearchCommand ToCommand(string connectionId) => new(connectionId);
}
