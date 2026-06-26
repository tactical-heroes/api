using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

public sealed class RegisterUserHandler(
    IUsersRepository identityUsersRepository,
    IPasswordHashingService passwordHashingService,
    IUserTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    private static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromHours(24);

    public async Task<Result<RegisterUserResult>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var passwordResult = PasswordPolicy.Validate(command.Password);

        if (passwordResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(passwordResult.Errors);
        }

        var existingUser = await identityUsersRepository.GetBySpecificationAsync(
            new UserByEmailSpecification(command.Email),
            cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure<RegisterUserResult>(
                Error.Conflict("User with this email already exists."));
        }

        var confirmationToken = identityTokenService.GenerateToken();
        var confirmationTokenHash = identityTokenService.HashToken(confirmationToken);
        var passwordHash = passwordHashingService.HashPassword(passwordResult.Value);

        var userResult = User.Register(
            command.Email,
            passwordHash,
            confirmationTokenHash,
            timeProvider.GetUtcNow().Add(ConfirmationTokenLifetime),
            confirmationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(userResult.Errors);
        }

        await identityUsersRepository.AddAsync(userResult.Value, cancellationToken);

        return Result.Success(new RegisterUserResult(userResult.Value.Id.Value));
    }
}
