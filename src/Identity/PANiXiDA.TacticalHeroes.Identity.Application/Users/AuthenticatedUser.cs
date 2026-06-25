namespace PANiXiDA.TacticalHeroes.Identity.Application.Users;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    bool IsConfirmed,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<AuthorizationClaim> Claims);
