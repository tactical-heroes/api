namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<Result<LoginSecurityProof>>;
