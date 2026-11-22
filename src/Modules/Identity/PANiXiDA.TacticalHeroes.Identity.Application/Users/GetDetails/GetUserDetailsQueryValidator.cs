using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetDetails;

public sealed class GetUserDetailsQueryValidator : AbstractValidator<GetUserDetailsQuery>
{
    public GetUserDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
