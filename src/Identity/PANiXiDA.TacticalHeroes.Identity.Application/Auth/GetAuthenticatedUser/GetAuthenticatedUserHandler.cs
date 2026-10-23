using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserHandler(IAuthenticatedUserReader reader)
    : IQueryHandler<GetAuthenticatedUserQuery, Result<AuthenticatedUserReadModel>>
{
    public Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        GetAuthenticatedUserQuery query,
        CancellationToken cancellationToken)
    {
        return reader.GetByIdAsync(query.UserId, cancellationToken);
    }
}
