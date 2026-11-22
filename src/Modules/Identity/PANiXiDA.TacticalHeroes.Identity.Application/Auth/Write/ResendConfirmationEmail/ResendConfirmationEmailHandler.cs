using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ResendConfirmationEmailCommand, Result>, IRequireTransaction
{
    public Task<Result> HandleAsync(
        ResendConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ResendConfirmationEmailAsync(
            command.Email,
            cancellationToken);
    }
}
