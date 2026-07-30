using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(expression: command => command.Email)
            .MustBeValidDomainValue(factory: Email.Create);

        RuleFor(expression: command => command.UserName)
            .MustBeValidDomainValue(factory: UserName.Create);

        RuleFor(expression: command => command.Password)
            .NotEmpty();
    }
}
