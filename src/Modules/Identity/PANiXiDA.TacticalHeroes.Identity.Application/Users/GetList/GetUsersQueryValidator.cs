using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new UsersFilterValidator());

        RuleFor(query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.SortingParameters)
            .NotNull()
            .SetValidator(new UserListItemReadModelSortingValidator());
    }
}
