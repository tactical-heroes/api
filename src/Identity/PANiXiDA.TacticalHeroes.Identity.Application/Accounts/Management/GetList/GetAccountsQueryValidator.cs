using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

public sealed class GetAccountsQueryValidator : AbstractValidator<GetAccountsQuery>
{
    public GetAccountsQueryValidator()
    {
        When(query => !string.IsNullOrWhiteSpace(query.Email), () =>
        {
            RuleFor(query => query.Email!)
                .MustBeValidDomainValue(Email.Create);
        });

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
