using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(expression: command => command.Email)
            .MustBeValidDomainValue(factory: Email.Create);

        RuleFor(expression: command => command.UserName)
            .MustBeValidDomainValue(factory: UserName.Create);

        RuleFor(expression: command => command.Password)
            .NotEmpty();

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

        RuleFor(expression: command => command.Status)
            .MustBeValidDomainValue(factory: UserStatus.Create);
    }
}
