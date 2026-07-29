using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

public sealed class ConfirmUserEmailHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ConfirmUserEmailCommand, Result>
{
    public Task<Result> HandleAsync(
        ConfirmUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ConfirmEmailAsync(
            command.UserId,
            command.EmailConfirmationToken,
            cancellationToken);
    }
}
