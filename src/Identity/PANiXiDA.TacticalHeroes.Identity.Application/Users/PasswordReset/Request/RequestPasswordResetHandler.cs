using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Request;

public sealed class RequestPasswordResetHandler(
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<RequestPasswordResetCommand, Result>
{
    public async Task<Result> HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        return await userCredentialsService.RequestPasswordResetAsync(
            command.Email,
            cancellationToken);
    }
}
