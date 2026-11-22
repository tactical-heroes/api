namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsQueryValidator : AbstractValidator<GetFactionsQuery>
{
    public GetFactionsQueryValidator()
    {
        RuleFor(query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.SortingParameters)
            .NotNull()
            .SetValidator(new FactionListItemReadModelSortingValidator());
    }
}
