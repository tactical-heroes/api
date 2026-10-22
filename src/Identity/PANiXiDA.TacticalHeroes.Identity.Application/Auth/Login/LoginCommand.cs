using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<Result<AuthenticatedUserReadModel>>;
