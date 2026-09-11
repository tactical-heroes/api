namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsQueryValidator : AbstractValidator<GetFactionSelectOptionsQuery>
{
    public GetFactionSelectOptionsQueryValidator()
    {
        RuleFor(query => query.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search == null ? null : query.Search.Trim())
            .Length(3, 128)
            .OverridePropertyName(nameof(GetFactionSelectOptionsQuery.Search));
    }
}
