using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

public sealed class LogoutHandler(IUserSessionService sessions)
    : ICommandHandler<LogoutCommand, Result>, IRequireTransaction
{
    public Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        return sessions.RevokeSessionAsync(
            userId: command.UserId,
            authorizationId: command.AuthorizationId,
            clientId: command.ClientId,
            cancellationToken: cancellationToken);
    }
}
