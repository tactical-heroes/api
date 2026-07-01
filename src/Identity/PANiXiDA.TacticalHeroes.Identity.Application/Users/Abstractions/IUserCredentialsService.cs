using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserCredentialsService
{
    Task<bool> CheckPasswordAsync(
        User user,
        string password,
        CancellationToken cancellationToken);

    Task<Result<UserGeneratedToken>> GenerateEmailConfirmationTokenAsync(
        User user,
        CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(
        User user,
        string confirmationToken,
        CancellationToken cancellationToken);

    Task<Result<UserGeneratedToken>> GeneratePasswordResetTokenAsync(
        User user,
        CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(
        User user,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken);
}
