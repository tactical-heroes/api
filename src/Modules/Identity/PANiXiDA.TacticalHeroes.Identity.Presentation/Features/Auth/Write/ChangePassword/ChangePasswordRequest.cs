namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ChangePassword;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
