namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : ICommand<Result>;
