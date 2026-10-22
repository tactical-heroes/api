using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string AccessToken,
    string CurrentPassword,
    string NewPassword) : ICommand<Result<AuthenticatedUserReadModel>>;
