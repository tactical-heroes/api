using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ChangePassword;

public sealed class ChangeUserPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ChangeUserPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ChangeUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
    }
}
