using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed class GetUserTokenPrincipalQueryValidator : AbstractValidator<GetUserTokenPrincipalQuery>
{
    public GetUserTokenPrincipalQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
