namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

public sealed record StartMatchSearchCommand(Guid BuildId, string Mode) : ICommand<Result>;
