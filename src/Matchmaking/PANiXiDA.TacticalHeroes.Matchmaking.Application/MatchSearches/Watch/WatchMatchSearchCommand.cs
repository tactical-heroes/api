using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed record WatchMatchSearchCommand : ICommand<Result<IMatchSearchStateSubscription>>;
