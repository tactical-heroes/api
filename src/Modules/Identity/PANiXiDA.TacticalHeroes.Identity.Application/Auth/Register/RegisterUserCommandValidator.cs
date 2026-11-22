using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.UserName)
            .MustBeValidDomainValue(UserName.Create);

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
