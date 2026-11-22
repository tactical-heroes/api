namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
