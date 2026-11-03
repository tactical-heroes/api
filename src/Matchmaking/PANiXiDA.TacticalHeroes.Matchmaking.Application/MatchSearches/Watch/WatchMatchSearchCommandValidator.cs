namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchCommandValidator : AbstractValidator<WatchMatchSearchCommand>
{
    public WatchMatchSearchCommandValidator()
    {
        RuleFor(command => command.ConnectionId).NotEmpty();
    }
}
