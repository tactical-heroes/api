namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed class GetUnitsQueryValidator : AbstractValidator<GetUnitsQuery>
{
    public GetUnitsQueryValidator()
    {
        RuleFor(expression: query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(expression: query => query.SortingParameters)
            .NotNull()
            .SetValidator(new UnitListItemReadModelSortingValidator());
    }
}
