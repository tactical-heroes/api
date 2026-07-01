using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class ResetPasswordHandler(
    IUsersRepository identityUsersRepository,
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await identityUsersRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User was not found."));
        }

        return await userCredentialsService.ResetPasswordAsync(
            user,
            command.PasswordResetToken,
            command.NewPassword,
            cancellationToken);
    }
}
