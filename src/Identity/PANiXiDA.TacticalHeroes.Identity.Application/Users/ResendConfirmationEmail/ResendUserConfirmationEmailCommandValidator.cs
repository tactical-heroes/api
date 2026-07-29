using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ResendConfirmationEmail;

public sealed class ResendUserConfirmationEmailCommandValidator : AbstractValidator<ResendUserConfirmationEmailCommand>
{
    public ResendUserConfirmationEmailCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
