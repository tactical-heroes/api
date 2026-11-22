namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;

public sealed record ConfirmMatchCommand(Guid FirstSearchId, Guid SecondSearchId) : ICommand<Result>;
