namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty();

        RuleFor(command => command.NewPassword)
            .NotEmpty();
    }
}
