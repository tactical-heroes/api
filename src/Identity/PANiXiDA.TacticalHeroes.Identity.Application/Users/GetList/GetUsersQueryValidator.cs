using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        When(predicate: query => !string.IsNullOrWhiteSpace(value: query.Email), action: () =>
        {
            RuleFor(expression: query => query.Email!)
                .MustBeValidDomainValue(factory: Email.Create);
        });

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
