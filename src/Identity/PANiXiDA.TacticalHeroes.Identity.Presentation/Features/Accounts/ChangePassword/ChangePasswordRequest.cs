namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ChangePassword;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
