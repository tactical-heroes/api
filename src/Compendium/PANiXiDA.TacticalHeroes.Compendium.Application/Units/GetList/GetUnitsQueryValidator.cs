namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed class GetUnitsQueryValidator : AbstractValidator<GetUnitsQuery>
{
    public GetUnitsQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.Sorting)
            .NotNull()
            .SetValidator(new UnitListItemReadModelSortingValidator());
    }
}
