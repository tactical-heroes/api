using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(expression: query => query.Filter)
            .NotNull()
            .SetValidator(new UsersFilterValidator());

        RuleFor(expression: query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(expression: query => query.SortingParameters)
            .NotNull()
            .SetValidator(new UserListItemReadModelSortingValidator());
    }
}
