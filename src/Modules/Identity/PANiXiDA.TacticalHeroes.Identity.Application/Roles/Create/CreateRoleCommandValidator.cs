using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Claims;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Create;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
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
