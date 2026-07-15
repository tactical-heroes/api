namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserCredentialsService
{
    Task<Result<Guid>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result<Guid>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result> ConfirmRegistrationAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken);

    Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(
        Guid userId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken);
}
