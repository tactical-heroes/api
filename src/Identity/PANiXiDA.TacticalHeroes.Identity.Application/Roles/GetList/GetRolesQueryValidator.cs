namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
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
