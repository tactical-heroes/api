using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class RequestPasswordResetHandler(
    IUsersRepository identityUsersRepository,
    IUserTokenService identityTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<RequestPasswordResetCommand, Result>
{
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<Result> HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var user = await identityUsersRepository.GetBySpecificationAsync(
            new UserByEmailSpecification(command.Email),
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
