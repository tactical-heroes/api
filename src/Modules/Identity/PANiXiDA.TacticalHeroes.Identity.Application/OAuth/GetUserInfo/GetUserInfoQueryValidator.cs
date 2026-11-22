using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetUserInfo;

public sealed class GetUserInfoQueryValidator : AbstractValidator<GetUserInfoQuery>
{
    public GetUserInfoQueryValidator()
    {
        RuleFor(query => query.UserId)
            .MustBeValidDomainValue(UserId.Create);
    }
}
