using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Cancel;

public sealed class CancelMatchSearchCommandValidator : AbstractValidator<CancelMatchSearchCommand>
{
    public CancelMatchSearchCommandValidator()
    {
        RuleFor(command => command.Id).MustBeValidDomainValue(MatchSearchId.Create);
    }
}
