using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Reset;

public sealed class ResetPasswordHandler(
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ResetPasswordAsync(
            command.UserId,
            command.PasswordResetToken,
            command.NewPassword,
            cancellationToken);
    }
}
