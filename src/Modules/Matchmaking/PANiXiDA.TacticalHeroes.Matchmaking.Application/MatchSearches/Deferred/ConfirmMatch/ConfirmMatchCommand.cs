namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Deferred.ConfirmMatch;

public sealed record ConfirmMatchCommand(Guid FirstSearchId, Guid SecondSearchId) : ICommand<Result>;
