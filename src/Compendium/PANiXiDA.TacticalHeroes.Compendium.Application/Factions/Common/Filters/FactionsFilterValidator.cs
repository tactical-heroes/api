using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;

public sealed class FactionsFilterValidator : AbstractValidator<FactionsFilter>
{
    public FactionsFilterValidator()
    {
        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(3, FactionName.MaxLength)
            .OverridePropertyName(nameof(FactionsFilter.Search));
    }
}
