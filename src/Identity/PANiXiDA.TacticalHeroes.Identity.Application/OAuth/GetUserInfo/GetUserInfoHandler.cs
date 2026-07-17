using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

public sealed class GetUserInfoHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<GetUserInfoQuery, Result<UserInfoReadModel>>
{
    public Task<Result<UserInfoReadModel>> HandleAsync(
        GetUserInfoQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetUserInfoByAccountIdAsync(
            query.AccountId,
            cancellationToken);
    }
}
