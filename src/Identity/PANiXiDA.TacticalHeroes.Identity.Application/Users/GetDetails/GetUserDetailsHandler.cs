using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

public sealed class GetUserDetailsHandler(IUsersReadRepository usersRepository)
    : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsReadModel>>
{
    public Task<Result<UserDetailsReadModel>> HandleAsync(
        GetUserDetailsQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetDetailsByIdAsync(
            query.Id,
            cancellationToken);
    }
}
