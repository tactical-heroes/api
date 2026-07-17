namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<Result<AuthenticatedAccountReadModel>>;
