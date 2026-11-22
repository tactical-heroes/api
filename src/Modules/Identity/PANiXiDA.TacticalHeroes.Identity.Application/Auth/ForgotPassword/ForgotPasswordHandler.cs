using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ForgotPasswordCommand, Result>, IRequireTransaction
{
    public Task<Result> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ForgotPasswordAsync(
            command.Email,
            cancellationToken);
    }
}
