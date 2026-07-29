using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ResetPassword;

public sealed class ResetUserPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResetUserPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ResetPasswordAsync(
            command.UserId,
            command.PasswordResetToken,
            command.NewPassword,
            cancellationToken);
    }
}
