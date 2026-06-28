using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserAuthenticationService
{
    Task<Result<AuthenticatedUserReadModel>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
