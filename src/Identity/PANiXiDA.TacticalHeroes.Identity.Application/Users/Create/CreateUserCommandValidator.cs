using PANiXiDA.TacticalHeroes.Identity.Application.Claims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.UserName)
            .MustBeValidDomainValue(UserName.Create);

        RuleFor(command => command.Password)
            .NotEmpty();

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

        RuleFor(command => command.RoleIds)
            .MustBeValidDomainValue(UserRoleAssignments.Create);

        RuleFor(command => command.Status)
            .MustBeValidDomainValue(UserStatus.Create);
    }
}
