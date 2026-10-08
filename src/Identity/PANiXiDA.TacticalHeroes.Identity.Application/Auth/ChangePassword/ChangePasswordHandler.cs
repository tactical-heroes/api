using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ChangePasswordCommand, Result<AuthenticatedUserReadModel>>
{
    public Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ChangePasswordAsync(
            userId: command.UserId,
            authorizationId: command.AuthorizationId,
            currentPassword: command.CurrentPassword,
            newPassword: command.NewPassword,
            cancellationToken: cancellationToken);
    }
}
