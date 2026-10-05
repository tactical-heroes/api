namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsQueryValidator : AbstractValidator<GetFactionsQuery>
{
    public GetFactionsQueryValidator()
    {
        RuleFor(expression: query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(expression: query => query.SortingParameters)
            .NotNull()
            .SetValidator(new FactionListItemReadModelSortingValidator());
    }
}
