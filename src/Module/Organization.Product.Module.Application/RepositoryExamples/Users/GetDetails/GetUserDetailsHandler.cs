using Organization.Product.Module.Application.Users.Abstractions;

namespace Organization.Product.Module.Application.Users.GetDetails;

public sealed class GetUserDetailsHandler(
    IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsReadModel>>
{
    public async Task<Result<UserDetailsReadModel>> HandleAsync(
        GetUserDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var user = await usersReadRepository.GetByIdAsync(query.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDetailsReadModel>(
                Error.NotFound($"User with id '{query.Id}' was not found."));
        }

        return Result.Success(user);
    }
}
