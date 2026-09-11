namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsQueryValidator : AbstractValidator<GetFactionsQuery>
{
    public GetFactionsQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());
    }
}
