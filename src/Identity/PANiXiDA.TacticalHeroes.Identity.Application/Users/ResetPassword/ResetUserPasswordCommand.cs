namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ResetPassword;

public sealed record ResetUserPasswordCommand(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
