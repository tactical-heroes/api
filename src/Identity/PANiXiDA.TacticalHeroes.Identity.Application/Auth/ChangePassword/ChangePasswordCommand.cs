using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : ICommand<Result<AuthenticatedUserReadModel>>;
