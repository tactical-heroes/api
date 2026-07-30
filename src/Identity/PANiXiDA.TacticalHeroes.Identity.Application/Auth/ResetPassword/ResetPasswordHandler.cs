using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

public sealed class ResetPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ResetPasswordAsync(
            userId: command.UserId,
            passwordResetToken: command.PasswordResetToken,
            newPassword: command.NewPassword,
            cancellationToken: cancellationToken);
    }
}
