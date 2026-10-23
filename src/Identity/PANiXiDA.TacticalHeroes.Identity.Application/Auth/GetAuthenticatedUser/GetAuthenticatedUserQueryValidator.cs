using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserQueryValidator : AbstractValidator<GetAuthenticatedUserQuery>
{
    public GetAuthenticatedUserQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
