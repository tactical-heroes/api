namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Cancel;

public sealed record CancelMatchSearchCommand(Guid Id) : ICommand<Result>;
