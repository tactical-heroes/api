namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Start;

public sealed record StartMatchSearchCommand(Guid BuildId, string Mode) : ICommand<Result>;
