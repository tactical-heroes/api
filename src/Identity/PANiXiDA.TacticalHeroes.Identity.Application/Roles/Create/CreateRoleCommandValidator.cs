using PANiXiDA.TacticalHeroes.Identity.Application.Claims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(RoleName.Create);

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
