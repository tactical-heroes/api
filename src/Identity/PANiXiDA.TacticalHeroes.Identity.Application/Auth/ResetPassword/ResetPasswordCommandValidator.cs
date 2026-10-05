using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(expression: command => command.UserId)
            .MustBeValidDomainValue(factory: UserId.Create);

        RuleFor(expression: command => command.PasswordResetToken)
            .NotEmpty();

        RuleFor(expression: command => command.NewPassword)
            .NotEmpty();
    }
}
