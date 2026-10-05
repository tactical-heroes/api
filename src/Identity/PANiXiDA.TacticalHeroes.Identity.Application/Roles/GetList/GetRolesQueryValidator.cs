namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
        RuleFor(expression: query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(expression: query => query.SortingParameters)
            .NotNull()
            .SetValidator(new RoleListItemReadModelSortingValidator());
    }
}
