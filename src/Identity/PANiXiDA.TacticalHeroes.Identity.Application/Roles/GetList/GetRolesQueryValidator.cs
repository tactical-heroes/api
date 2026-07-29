namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
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
