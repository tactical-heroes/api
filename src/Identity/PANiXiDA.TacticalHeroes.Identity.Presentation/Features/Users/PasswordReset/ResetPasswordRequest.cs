namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.PasswordReset;

internal sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
