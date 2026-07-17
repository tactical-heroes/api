namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid AccountId,
    string CurrentPassword,
    string NewPassword) : ICommand<Result>;
