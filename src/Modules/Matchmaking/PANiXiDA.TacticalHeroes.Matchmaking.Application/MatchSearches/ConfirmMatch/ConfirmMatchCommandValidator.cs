using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;

public sealed class ConfirmMatchCommandValidator : AbstractValidator<ConfirmMatchCommand>
{
    public ConfirmMatchCommandValidator()
    {
        RuleFor(command => command.FirstSearchId).MustBeValidDomainValue(MatchSearchId.Create);
        RuleFor(command => command.SecondSearchId).MustBeValidDomainValue(MatchSearchId.Create);
        RuleFor(command => command).MustBeValidDomainValue(command =>
            MatchmakingPair.Create(command.FirstSearchId, command.SecondSearchId));
    }
}
