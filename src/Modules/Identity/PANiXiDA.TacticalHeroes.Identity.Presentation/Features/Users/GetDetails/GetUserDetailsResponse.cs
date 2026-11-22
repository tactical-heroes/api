using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.GetDetails;

public sealed record GetUserDetailsResponse(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName,
    IReadOnlyCollection<Claim> Claims,
    IReadOnlyCollection<Guid> RoleIds);
