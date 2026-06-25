namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Confirm;

public sealed class ConfirmRegistrationCommandValidator : AbstractValidator<ConfirmRegistrationCommand>
{
    public ConfirmRegistrationCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.ConfirmationToken)
            .NotEmpty();
    }
}
