namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsQueryValidator : AbstractValidator<GetFactionsQuery>
{
    public GetFactionsQueryValidator()
    {
        RuleFor(expression: query => query.Pagination)
            .NotNull();

        When(predicate: query => query.Pagination is not null, action: () =>
        {
            RuleFor(expression: query => query.Pagination.PageNumber)
                .GreaterThan(valueToCompare: 0);

            RuleFor(expression: query => query.Pagination.PageSize)
                .GreaterThan(valueToCompare: 0);
        });
    }
}
