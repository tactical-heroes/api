using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

public sealed class ConfirmEmailHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ConfirmEmailCommand, Result>
{
    public Task<Result> HandleAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ConfirmEmailAsync(
            command.UserId,
            command.EmailConfirmationToken,
            cancellationToken);
    }
}
