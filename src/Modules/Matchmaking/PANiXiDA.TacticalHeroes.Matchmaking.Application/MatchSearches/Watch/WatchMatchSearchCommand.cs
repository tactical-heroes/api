namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Watch;

public sealed record WatchMatchSearchCommand(string ConnectionId) : ICommand<Result>;
