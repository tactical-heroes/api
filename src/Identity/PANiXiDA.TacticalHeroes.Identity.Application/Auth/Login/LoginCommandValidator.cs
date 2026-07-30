using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(expression: command => command.Email)
            .MustBeValidDomainValue(factory: Email.Create);

        RuleFor(expression: command => command.Password)
            .NotEmpty();
    }
}
