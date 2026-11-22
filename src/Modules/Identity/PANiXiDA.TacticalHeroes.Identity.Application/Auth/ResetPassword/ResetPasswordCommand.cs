namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
