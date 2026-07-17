using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IAccountCredentialsService
{
    Task<Result<Guid>> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedAccountReadModel>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(
        Guid accountId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    Task<Result> ConfirmAsync(
        Guid accountId,
        string emailConfirmationToken,
        CancellationToken cancellationToken);

    Task<Result> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(
        Guid accountId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken);
}
