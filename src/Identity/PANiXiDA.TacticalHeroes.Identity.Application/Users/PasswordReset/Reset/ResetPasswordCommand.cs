namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Reset;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
