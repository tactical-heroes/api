namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

public sealed record ResetUserPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
