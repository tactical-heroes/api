using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed class ExchangeTokenQueryValidator : AbstractValidator<ExchangeTokenQuery>
{
    public ExchangeTokenQueryValidator()
    {
        RuleFor(expression: query => query.UserId)
            .MustBeValidDomainValue(factory: UserId.Create);
    }
}
