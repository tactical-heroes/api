using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new UsersFilterValidator());

        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());
    }
}
