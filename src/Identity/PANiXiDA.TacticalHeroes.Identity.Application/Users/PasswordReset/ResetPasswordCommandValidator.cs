using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Policies;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.PasswordResetToken)
            .NotEmpty();

        RuleFor(command => command.NewPassword)
            .MustBeValidDomainValue(PasswordPolicy.Validate);
    }
}
