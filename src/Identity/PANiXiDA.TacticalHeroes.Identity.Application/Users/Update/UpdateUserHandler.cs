using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

public sealed class UpdateUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<UpdateUserCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var userResult = UserMapper.ToDomain(
            id: command.Id,
            email: command.Email,
            isConfirmed: command.IsConfirmed,
            roleIds: [],
            claims: command.Claims.Select(claim => (claim.Type, claim.Value)));
        var userNameResult = UserName.Create(value: command.UserName);
        var statusResult = UserStatus.Create(value: command.Status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        return validationResult.IsFailure
            ? Task.FromResult(Result.Failure(errors: validationResult.Errors))
            : usersRepository.UpdateAsync(
                user: userResult.Value,
                userName: userNameResult.Value,
                status: statusResult.Value,
                cancellationToken: cancellationToken);
    }
}
