using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.PasswordReset;

public sealed class RequestPasswordResetHandler(
    IIdentityUsersRepository identityUsersRepository,
    IIdentityTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<RequestPasswordResetCommand, Result>
{
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<Result> HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Errors);
        }

        var user = await identityUsersRepository.GetByEmailAsync(
            emailResult.Value,
            cancellationToken);

        if (user is null || !user.IsConfirmed)
        {
            return Result.Success();
        }

        var passwordResetToken = identityTokenService.GenerateToken();
        var result = user.RequestPasswordReset(
            identityTokenService.HashToken(passwordResetToken),
            timeProvider.GetUtcNow().Add(PasswordResetTokenLifetime),
            passwordResetToken);

        if (result.IsFailure)
        {
            return result;
        }

        await identityUsersRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
