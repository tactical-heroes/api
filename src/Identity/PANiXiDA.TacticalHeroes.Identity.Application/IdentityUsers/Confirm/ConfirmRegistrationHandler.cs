using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Confirm;

public sealed class ConfirmRegistrationHandler(
    IIdentityUsersRepository identityUsersRepository,
    IIdentityTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmRegistrationCommand, Result>
{
    public async Task<Result> HandleAsync(
        ConfirmRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = IdentityUserId.Create(command.UserId);

        if (userIdResult.IsFailure)
        {
            return Result.Failure(userIdResult.Errors);
        }

        var user = await identityUsersRepository.GetByIdAsync(
            userIdResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("Identity user was not found."));
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
