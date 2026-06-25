using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.PasswordReset;

public sealed class ResetPasswordHandler(
    IIdentityUsersRepository identityUsersRepository,
    IIdentityTokenService identityTokenService,
    IPasswordHashingService passwordHashingService,
    TimeProvider timeProvider)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = IdentityUserId.Create(command.UserId);
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
            return Result.Failure(Error.NotFound("Identity user was not found."));
        }

        var result = user.ResetPassword(
            identityTokenService.HashToken(command.PasswordResetToken),
            passwordHashingService.HashPassword(passwordResult.Value),
            timeProvider.GetUtcNow());

        if (result.IsFailure)
        {
            return result;
        }

        await identityUsersRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
