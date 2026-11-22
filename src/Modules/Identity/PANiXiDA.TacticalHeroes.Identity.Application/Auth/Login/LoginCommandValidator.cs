using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
