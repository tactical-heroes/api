using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
