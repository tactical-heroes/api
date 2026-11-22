using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.Authorize;

public sealed class AuthorizeQueryValidator : AbstractValidator<AuthorizeQuery>
{
    public AuthorizeQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
