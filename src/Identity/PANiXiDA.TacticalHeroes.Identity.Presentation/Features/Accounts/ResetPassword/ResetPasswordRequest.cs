namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResetPassword;

public sealed record ResetPasswordRequest(
    Guid AccountId,
    string PasswordResetToken,
    string NewPassword);
