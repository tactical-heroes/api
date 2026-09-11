namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());
    }
}
