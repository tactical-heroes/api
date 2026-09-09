using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed class CreateUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var userResult = UserMapper.ToDomain(
            id: UserId.New().Value,
            email: command.Email,
            isConfirmed: command.IsConfirmed,
            roleIds: [],
            claims: command.Claims.Select(claim => (claim.Type, claim.Value)));
        var userNameResult = UserName.Create(value: command.UserName);
        var statusResult = UserStatus.Create(value: command.Status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        return validationResult.IsFailure
            ? Task.FromResult(Result.Failure<Guid>(errors: validationResult.Errors))
            : usersRepository.AddAsync(
                user: userResult.Value,
                userName: userNameResult.Value,
                password: command.Password,
                status: statusResult.Value,
                cancellationToken: cancellationToken);
    }
}
