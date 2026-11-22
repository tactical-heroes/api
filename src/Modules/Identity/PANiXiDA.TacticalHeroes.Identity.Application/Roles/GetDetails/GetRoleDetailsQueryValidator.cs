using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed class GetRoleDetailsQueryValidator : AbstractValidator<GetRoleDetailsQuery>
{
    public GetRoleDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(RoleId.Create);
    }
}
