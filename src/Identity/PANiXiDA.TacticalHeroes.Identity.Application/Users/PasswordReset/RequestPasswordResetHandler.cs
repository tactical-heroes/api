using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class RequestPasswordResetHandler(
    IUsersRepository identityUsersRepository,
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<RequestPasswordResetCommand, Result>
{
    public async Task<Result> HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var user = await identityUsersRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || !user.ConfirmationStatus.IsConfirmed)
        {
            return Result.Success();
        }

        var passwordResetTokenResult = await userCredentialsService.GeneratePasswordResetTokenAsync(
            user,
            cancellationToken);

        if (passwordResetTokenResult.IsFailure)
        {
            return passwordResetTokenResult;
        }

        var result = user.RequestPasswordReset(
            passwordResetTokenResult.Value.Value,
            passwordResetTokenResult.Value.ExpiresAtUtc);

        if (result.IsFailure)
        {
            return result;
        }

        return await identityUsersRepository.UpdateAsync(user, cancellationToken);
    }
}
