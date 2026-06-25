using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;

namespace Organization.Product.Module.Application.Users.Update;

public sealed class UpdateUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<UpdateUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = UserId.Create(command.Id);

        if (idResult.IsFailure)
        {
            return Result.Failure(idResult.Errors);
        }

        var user = await usersRepository.GetByIdAsync(
            idResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(
                Error.NotFound($"User with id '{command.Id}' was not found.")
                    .WithField(nameof(User)));
        }

        var updateResult = user.Update(
            command.Role,
            command.Name,
            command.Email,
            command.Phone,
            command.BirthDate,
            command.Avatar);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await usersRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
