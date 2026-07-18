using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ChangePasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
    }
}
