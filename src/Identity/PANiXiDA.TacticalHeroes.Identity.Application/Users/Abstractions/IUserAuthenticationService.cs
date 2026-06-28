namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserAuthenticationService
{
    Task<Result<AuthenticatedUser>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
