using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

public sealed class RegisterUserHandler(
    IUsersRepository identityUsersRepository,
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    public async Task<Result<RegisterUserResult>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await identityUsersRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure<RegisterUserResult>(
                Error.Conflict("User with this email already exists."));
        }

        var userResult = User.Register(command.Email);

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(userResult.Errors);
        }

        var addUserResult = await identityUsersRepository.AddAsync(
            userResult.Value,
            command.Password,
            cancellationToken);

        if (addUserResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(addUserResult.Errors);
        }

        var confirmationTokenResult = await userCredentialsService.GenerateEmailConfirmationTokenAsync(
            userResult.Value,
            cancellationToken);

        if (confirmationTokenResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(confirmationTokenResult.Errors);
        }

        var requestConfirmationResult = userResult.Value.RequestAccountConfirmation(
            confirmationTokenResult.Value.Value,
            confirmationTokenResult.Value.ExpiresAtUtc);

        if (requestConfirmationResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(requestConfirmationResult.Errors);
        }

        var updateUserResult = await identityUsersRepository.UpdateAsync(
            userResult.Value,
            cancellationToken);

        if (updateUserResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(updateUserResult.Errors);
        }

        return Result.Success(new RegisterUserResult(userResult.Value.Id.Value));
    }
}
