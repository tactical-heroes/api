namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed class GetHeroesQueryValidator : AbstractValidator<GetHeroesQuery>
{
    public GetHeroesQueryValidator()
    {
        RuleFor(query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.SortingParameters)
            .NotNull()
            .SetValidator(new HeroListItemReadModelSortingValidator());
    }
}
