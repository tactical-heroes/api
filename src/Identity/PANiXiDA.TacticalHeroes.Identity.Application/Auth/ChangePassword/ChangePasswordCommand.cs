namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand<Result>;
