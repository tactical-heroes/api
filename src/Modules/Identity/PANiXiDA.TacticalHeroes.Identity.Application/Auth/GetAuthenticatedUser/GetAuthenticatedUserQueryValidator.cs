using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserQueryValidator : AbstractValidator<GetAuthenticatedUserQuery>
{
    public GetAuthenticatedUserQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(query => query.SecurityStamp).NotEmpty();
    }
}
