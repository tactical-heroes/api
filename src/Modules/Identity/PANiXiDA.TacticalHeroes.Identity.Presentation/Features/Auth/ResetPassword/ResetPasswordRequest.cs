namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ResetPassword;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
