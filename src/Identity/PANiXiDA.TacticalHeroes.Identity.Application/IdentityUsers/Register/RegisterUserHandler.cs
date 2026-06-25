using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Register;

public sealed class RegisterUserHandler(
    IIdentityUsersRepository identityUsersRepository,
    IPasswordHashingService passwordHashingService,
    IIdentityTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    private static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromHours(24);

    public async Task<Result<RegisterUserResult>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        var passwordResult = PasswordPolicy.Validate(command.Password);
        var validationResult = Result.Combine(emailResult, passwordResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<RegisterUserResult>(validationResult.Errors);
        }

        var existingUser = await identityUsersRepository.GetByEmailAsync(
            emailResult.Value,
            cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure<RegisterUserResult>(
                Error.Conflict("User with this email already exists."));
        }

        var confirmationToken = identityTokenService.GenerateToken();
        var confirmationTokenHash = identityTokenService.HashToken(confirmationToken);
        var passwordHash = passwordHashingService.HashPassword(passwordResult.Value);

        var userResult = IdentityUser.Register(
            emailResult.Value,
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
