namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.PasswordReset;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
