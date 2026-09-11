namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed class GetHeroesQueryValidator : AbstractValidator<GetHeroesQuery>
{
    public GetHeroesQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull();

        When(query => query.Pagination is not null, () =>
        {
            RuleFor(query => query.Pagination.PageNumber)
                .GreaterThan(0);

            RuleFor(query => query.Pagination.PageSize)
                .GreaterThan(0);
        });
    }
}
