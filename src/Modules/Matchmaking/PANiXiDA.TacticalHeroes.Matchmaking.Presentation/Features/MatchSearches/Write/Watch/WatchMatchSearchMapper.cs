using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Write.Watch;

internal static class WatchMatchSearchMapper
{
    internal static WatchMatchSearchCommand ToCommand(string connectionId) => new(connectionId);
}
