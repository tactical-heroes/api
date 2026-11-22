using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken;

public sealed class ExchangeTokenQueryValidator : AbstractValidator<ExchangeTokenQuery>
{
    public ExchangeTokenQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
