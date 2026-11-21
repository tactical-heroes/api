namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed record WatchMatchSearchCommand(string ConnectionId) : ICommand<Result>;
