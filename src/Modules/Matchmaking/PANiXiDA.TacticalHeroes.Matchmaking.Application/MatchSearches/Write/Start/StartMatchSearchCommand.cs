namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Start;

public sealed record StartMatchSearchCommand(Guid BuildId, string Mode) : ICommand<Result>;
