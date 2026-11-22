using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.EmailConfirmationToken)
            .NotEmpty();
    }
}
