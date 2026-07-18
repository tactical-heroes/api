namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
