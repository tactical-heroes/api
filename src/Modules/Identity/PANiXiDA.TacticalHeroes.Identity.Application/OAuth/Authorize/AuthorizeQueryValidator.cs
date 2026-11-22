using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Authorize;

public sealed class AuthorizeQueryValidator : AbstractValidator<AuthorizeQuery>
{
    public AuthorizeQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
