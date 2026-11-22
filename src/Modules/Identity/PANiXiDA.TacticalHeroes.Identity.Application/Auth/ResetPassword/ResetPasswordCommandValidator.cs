using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResetPassword;

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
