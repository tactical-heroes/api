using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

public sealed class ResendUserConfirmationEmailHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResendUserConfirmationEmailCommand, Result>
{
    public Task<Result> HandleAsync(
        ResendUserConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ResendConfirmationEmailAsync(
            command.Email,
            cancellationToken);
    }
}
