namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchCommandValidator : AbstractValidator<WatchMatchSearchCommand>
{
    public WatchMatchSearchCommandValidator()
    {
        RuleFor(command => command.ConnectionId).NotEmpty();
    }
}
