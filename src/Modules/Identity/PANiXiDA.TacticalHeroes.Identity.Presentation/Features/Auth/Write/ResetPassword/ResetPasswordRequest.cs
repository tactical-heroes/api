namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ResetPassword;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string PasswordResetToken,
    string NewPassword);
