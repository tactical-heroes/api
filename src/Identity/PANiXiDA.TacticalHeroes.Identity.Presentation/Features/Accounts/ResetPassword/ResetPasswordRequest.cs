namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResetPassword;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
