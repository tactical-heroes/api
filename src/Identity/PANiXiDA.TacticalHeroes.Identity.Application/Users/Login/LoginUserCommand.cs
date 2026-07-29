namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<Result<AuthenticatedUserReadModel>>;
