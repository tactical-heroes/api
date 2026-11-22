namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<Result<LoginSecurityProof>>;
