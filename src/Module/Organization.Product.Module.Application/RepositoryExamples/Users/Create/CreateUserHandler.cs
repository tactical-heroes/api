using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;

namespace Organization.Product.Module.Application.Users.Create;

public sealed class CreateUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var userResult = User.Create(
            command.Role,
            command.Name,
            command.Email,
            command.Phone,
            command.BirthDate,
            command.Avatar);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Errors);
        }

        await usersRepository.AddAsync(userResult.Value, cancellationToken);

        return Result.Success(userResult.Value.Id.Value);
    }
}
