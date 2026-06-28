namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authenticate;

public sealed record AuthenticateUserCommand(
    string Email,
    string Password)
    : ICommand<Result<AuthenticatedUserReadModel>>;
