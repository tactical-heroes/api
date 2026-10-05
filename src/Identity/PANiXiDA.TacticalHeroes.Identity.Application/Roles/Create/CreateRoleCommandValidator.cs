using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(expression: command => command.Name)
            .MustBeValidDomainValue(factory: RoleName.Create);

        RuleFor(expression: command => command.Claims)
            .NotNull();

        RuleForEach(expression: command => command.Claims)
            .ChildRules(action: claim =>
            {
                claim.RuleFor(expression: item => item.Type)
                    .MustBeValidDomainValue(factory: ClaimType.Create);

                claim.RuleFor(expression: item => item.Value)
                    .MustBeValidDomainValue(factory: ClaimValue.Create);
            });
    }
}
