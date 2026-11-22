using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Claims;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Update;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(RoleId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(RoleName.Create);

        RuleFor(command => command.IsDefault)
            .MustBeValidDomainValue(RoleDefaultFlag.Create);

        RuleFor(command => command.Claims)
            .NotNull();

        RuleForEach(command => command.Claims)
            .ChildRules(claim =>
            {
                claim.RuleFor(item => item.Type)
                    .MustBeValidDomainValue(ClaimType.Create);

                claim.RuleFor(item => item.Value)
                    .MustBeValidDomainValue(ClaimValue.Create);
            })
            .SetValidator(new ClaimCatalogValidator());
    }
}
