namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<Result<AuthenticatedUserReadModel>>;
