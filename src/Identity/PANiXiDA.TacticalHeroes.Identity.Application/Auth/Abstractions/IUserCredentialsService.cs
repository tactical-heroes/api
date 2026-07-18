using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IUserCredentialsService
{
    Task<Result<Guid>> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedUserReadModel>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(
        Guid userId,
        string emailConfirmationToken,
        CancellationToken cancellationToken);

    Task<Result> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(
        Guid userId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken);
}
