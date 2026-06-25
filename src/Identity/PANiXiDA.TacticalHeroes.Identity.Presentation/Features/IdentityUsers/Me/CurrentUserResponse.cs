namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Me;

internal sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
