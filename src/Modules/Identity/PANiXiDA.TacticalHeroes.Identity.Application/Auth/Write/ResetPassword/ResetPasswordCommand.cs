namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
