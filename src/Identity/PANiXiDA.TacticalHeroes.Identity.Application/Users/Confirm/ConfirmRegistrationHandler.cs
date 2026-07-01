using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

public sealed class ConfirmRegistrationHandler(
    IUsersRepository identityUsersRepository,
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<ConfirmRegistrationCommand, Result>
{
    public async Task<Result> HandleAsync(
        ConfirmRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var user = await identityUsersRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User was not found."));
        }

        var confirmEmailResult = await userCredentialsService.ConfirmEmailAsync(
            user,
            command.ConfirmationToken,
            cancellationToken);

        if (confirmEmailResult.IsFailure)
        {
            return confirmEmailResult;
        }

        var result = user.ConfirmRegistration();

        if (result.IsFailure)
        {
            return result;
        }

        return await identityUsersRepository.UpdateAsync(user, cancellationToken);
    }
}
