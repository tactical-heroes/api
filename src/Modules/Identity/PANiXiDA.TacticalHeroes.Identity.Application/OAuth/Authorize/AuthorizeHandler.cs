using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Authorize;

public sealed class AuthorizeHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<AuthorizeQuery, Result<AuthorizeReadModel>>
{
    public async Task<Result<AuthorizeReadModel>> HandleAsync(
        AuthorizeQuery query,
        CancellationToken cancellationToken)
    {
        var result = await usersRepository.CheckAvailabilityByUserIdAsync(
            query.UserId,
            cancellationToken);

        return result.IsFailure
            ? Result.Failure<AuthorizeReadModel>(errors: result.Errors)
            : Result.Success(value: new AuthorizeReadModel(UserId: query.UserId));
    }
}
