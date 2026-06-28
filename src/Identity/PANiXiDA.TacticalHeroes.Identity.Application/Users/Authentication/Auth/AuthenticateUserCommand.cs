namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Auth;

public sealed record AuthenticateUserCommand(
    string Email,
    string Password)
    : ICommand<Result<AuthenticatedUserReadModel>>;
