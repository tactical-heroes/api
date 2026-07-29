namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ChangePassword;

public sealed record ChangeUserPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand<Result>;
