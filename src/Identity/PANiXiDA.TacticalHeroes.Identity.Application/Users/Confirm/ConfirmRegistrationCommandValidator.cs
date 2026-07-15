using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

public sealed class ConfirmRegistrationCommandValidator : AbstractValidator<ConfirmRegistrationCommand>
{
    public ConfirmRegistrationCommandValidator()
    {
        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.ConfirmationToken)
            .NotEmpty();
    }
}
