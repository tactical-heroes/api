using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
