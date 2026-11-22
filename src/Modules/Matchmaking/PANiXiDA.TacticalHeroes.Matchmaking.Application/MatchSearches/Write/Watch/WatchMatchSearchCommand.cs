namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Watch;

public sealed record WatchMatchSearchCommand(string ConnectionId) : ICommand<Result>;
