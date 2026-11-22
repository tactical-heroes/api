using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed class GetUserTokenPrincipalHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<GetUserTokenPrincipalQuery, Result<OAuthUserTokenPrincipalReadModel>>
{
    public Task<Result<OAuthUserTokenPrincipalReadModel>> HandleAsync(
        GetUserTokenPrincipalQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetTokenPrincipalByUserIdAsync(
            query.UserId,
            cancellationToken);
    }
}
