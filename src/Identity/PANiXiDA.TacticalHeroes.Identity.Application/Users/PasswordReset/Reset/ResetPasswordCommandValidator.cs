using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Reset;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.PasswordResetToken)
            .NotEmpty();

        RuleFor(command => command.NewPassword)
            .NotEmpty();
    }
}
