using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

public sealed class ConfirmRegistrationHandler(
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<ConfirmRegistrationCommand, Result>
{
    public Task<Result> HandleAsync(
        ConfirmRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ConfirmRegistrationAsync(
            command.UserId,
            command.ConfirmationToken,
            cancellationToken);
    }
}
