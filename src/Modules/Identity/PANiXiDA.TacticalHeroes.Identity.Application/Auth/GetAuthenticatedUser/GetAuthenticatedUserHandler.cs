using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserHandler(IAuthenticatedUserService service, IUserSessionService sessions)
    : IQueryHandler<GetAuthenticatedUserQuery, Result<AuthenticatedUserReadModel>>
{
    public async Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        GetAuthenticatedUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await service.GetByIdAsync(query.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return user;
        }

        if (user.Value.Id != query.UserId
            || string.IsNullOrEmpty(query.SecurityStamp)
            || string.IsNullOrEmpty(user.Value.SecurityStamp))
        {
            return InvalidSession();
        }

        var isValid = query.AuthorizationId is { } authorizationId
            ? await sessions.IsAuthorizationValidAsync(
                userId: query.UserId,
                authorizationId: authorizationId,
                cancellationToken: cancellationToken)
            : string.Equals(query.SecurityStamp, user.Value.SecurityStamp, StringComparison.Ordinal);

        return isValid ? user : InvalidSession();
    }

    private static Result<AuthenticatedUserReadModel> InvalidSession() =>
        Result.Failure<AuthenticatedUserReadModel>(Error.Unauthorized(message: "User session is invalid."));
}
