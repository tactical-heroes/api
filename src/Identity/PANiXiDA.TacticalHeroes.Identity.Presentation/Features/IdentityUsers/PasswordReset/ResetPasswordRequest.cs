namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.PasswordReset;

internal sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
