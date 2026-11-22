using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetUserInfo;

public sealed class GetUserInfoHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<GetUserInfoQuery, Result<UserInfoReadModel>>
{
    public Task<Result<UserInfoReadModel>> HandleAsync(
        GetUserInfoQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetUserInfoByUserIdAsync(
            query.UserId,
            cancellationToken);
    }
}
