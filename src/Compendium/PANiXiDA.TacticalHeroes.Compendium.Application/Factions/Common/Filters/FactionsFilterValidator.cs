using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;

public sealed class FactionsFilterValidator : AbstractValidator<FactionsFilter>
{
    public FactionsFilterValidator()
    {
        RuleFor(expression: filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(min: 3, max: FactionName.MaxLength)
            .OverridePropertyName(propertyName: nameof(FactionsFilter.Search));
    }
}
