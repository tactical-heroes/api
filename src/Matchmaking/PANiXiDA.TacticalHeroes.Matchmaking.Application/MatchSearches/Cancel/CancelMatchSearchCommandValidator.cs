using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Cancel;

public sealed class CancelMatchSearchCommandValidator : AbstractValidator<CancelMatchSearchCommand>
{
    public CancelMatchSearchCommandValidator()
    {
        RuleFor(command => command.Id).MustBeValidDomainValue(MatchSearchId.Create);
    }
}
