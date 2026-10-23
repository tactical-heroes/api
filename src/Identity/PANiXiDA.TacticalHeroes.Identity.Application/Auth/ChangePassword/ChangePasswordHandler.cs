using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandler(IUserCredentialsService userCredentialsService, ICurrentUser currentUser)
    : ICommandHandler<ChangePasswordCommand, Result<AuthenticatedUserReadModel>>, IRequireTransaction
{
    public Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var accessToken = currentUser.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Task.FromResult(Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Unauthorized(message: "Current session is invalid.")));
        }

        return userCredentialsService.ChangePasswordAsync(
            userId: command.UserId,
            accessToken: accessToken,
            currentPassword: command.CurrentPassword,
            newPassword: command.NewPassword,
            cancellationToken: cancellationToken);
    }
}
