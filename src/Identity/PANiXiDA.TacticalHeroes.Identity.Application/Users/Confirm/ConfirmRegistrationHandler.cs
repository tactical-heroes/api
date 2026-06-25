using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

public sealed class ConfirmRegistrationHandler(
    IUsersRepository identityUsersRepository,
    IUserTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmRegistrationCommand, Result>
{
    public async Task<Result> HandleAsync(
        ConfirmRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(command.UserId);

        if (userIdResult.IsFailure)
        {
            return Result.Failure(userIdResult.Errors);
        }

        var user = await identityUsersRepository.GetByIdAsync(
            userIdResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User was not found."));
        }

        var result = user.ConfirmRegistration(
            identityTokenService.HashToken(command.ConfirmationToken),
            timeProvider.GetUtcNow());

        if (result.IsFailure)
        {
            return result;
        }

        await identityUsersRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
