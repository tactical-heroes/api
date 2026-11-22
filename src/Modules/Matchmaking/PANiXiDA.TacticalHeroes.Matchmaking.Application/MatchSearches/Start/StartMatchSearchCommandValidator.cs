using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

public sealed class StartMatchSearchCommandValidator : AbstractValidator<StartMatchSearchCommand>
{
    public StartMatchSearchCommandValidator()
    {
        RuleFor(command => command.BuildId).MustBeValidDomainValue(BuildId.Create);
        RuleFor(command => command.Mode).MustBeValidDomainValue(MatchmakingMode.Create);
    }
}
