using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsQueryValidator : AbstractValidator<GetFactionSelectOptionsQuery>
{
    public GetFactionSelectOptionsQueryValidator()
    {
        RuleFor(query => query.Limit)
            .NotNull()
            .SetValidator(new LimitParametersValidator());

        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new FactionsFilterValidator());
    }
}
