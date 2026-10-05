using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(expression: command => command.UserId)
            .MustBeValidDomainValue(factory: UserId.Create);

        RuleFor(expression: command => command.EmailConfirmationToken)
            .NotEmpty();
    }
}
