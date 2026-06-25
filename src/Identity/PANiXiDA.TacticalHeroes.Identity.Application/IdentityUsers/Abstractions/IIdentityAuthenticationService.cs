namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;

public interface IIdentityAuthenticationService
{
    Task<Result<AuthenticatedIdentityUser>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedIdentityUser>> GetConfirmedUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
