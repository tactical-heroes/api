namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid AccountId,
    string PasswordResetToken,
    string NewPassword) : ICommand<Result>;
