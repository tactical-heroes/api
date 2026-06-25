using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;

namespace Organization.Product.Module.Application.Users.Delete;

public sealed class DeleteUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(command.Id);

        if (userIdResult.IsFailure)
        {
            return Result.Failure(userIdResult.Errors);
        }

        var user = await usersRepository.GetByIdAsync(
            userIdResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(
                Error.NotFound($"User with id '{command.Id}' was not found."));
        }

        await usersRepository.DeleteAsync(user, cancellationToken);

        return Result.Success();
    }
}
