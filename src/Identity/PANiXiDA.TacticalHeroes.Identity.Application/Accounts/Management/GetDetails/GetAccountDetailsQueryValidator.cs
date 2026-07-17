using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;

public sealed class GetAccountDetailsQueryValidator : AbstractValidator<GetAccountDetailsQuery>
{
    public GetAccountDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
