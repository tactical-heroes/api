namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers;

public sealed record AuthenticatedIdentityUser(
    Guid Id,
    string Email,
    bool IsConfirmed,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
