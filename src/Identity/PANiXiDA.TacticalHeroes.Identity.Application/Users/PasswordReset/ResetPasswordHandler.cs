using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Policies;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class ResetPasswordHandler(
    IUsersRepository identityUsersRepository,
    IUserTokenService identityTokenService,
    IPasswordHashingService passwordHashingService,
    TimeProvider timeProvider)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(command.UserId);
        var passwordResult = PasswordPolicy.Validate(command.NewPassword);
        var validationResult = Result.Combine(userIdResult, passwordResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var user = await identityUsersRepository.GetByIdAsync(
            userIdResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User was not found."));
        }

        var result = user.ResetPassword(
            identityTokenService.HashToken(command.PasswordResetToken),
            passwordHashingService.HashPassword(command.NewPassword),
            timeProvider.GetUtcNow());

        if (result.IsFailure)
        {
            return result;
        }

        await identityUsersRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
