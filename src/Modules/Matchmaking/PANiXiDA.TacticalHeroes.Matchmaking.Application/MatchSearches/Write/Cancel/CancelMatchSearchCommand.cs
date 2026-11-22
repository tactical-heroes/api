namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Cancel;

public sealed record CancelMatchSearchCommand(Guid Id) : ICommand<Result>;
