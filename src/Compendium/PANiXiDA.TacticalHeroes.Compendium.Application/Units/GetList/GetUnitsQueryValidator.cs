namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed class GetUnitsQueryValidator : AbstractValidator<GetUnitsQuery>
{
    public GetUnitsQueryValidator()
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
