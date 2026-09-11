namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed class GetHeroesQueryValidator : AbstractValidator<GetHeroesQuery>
{
    public GetHeroesQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());
    }
}
