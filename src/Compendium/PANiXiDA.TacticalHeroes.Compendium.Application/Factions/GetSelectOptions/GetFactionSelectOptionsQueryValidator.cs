using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsQueryValidator : AbstractValidator<GetFactionSelectOptionsQuery>
{
    public GetFactionSelectOptionsQueryValidator()
    {
        RuleFor(query => query.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new FactionsFilterValidator());
    }
}
