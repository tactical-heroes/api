using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ConfirmEmail;

public sealed class ConfirmEmailHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ConfirmEmailCommand, Result>, IRequireTransaction
{
    public Task<Result> HandleAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ConfirmEmailAsync(
            userId: command.UserId,
            emailConfirmationToken: command.EmailConfirmationToken,
            cancellationToken: cancellationToken);
    }
}
