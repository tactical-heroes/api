using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Update;

public sealed record UpdateUserRequest(
    string Email,
    string UserName,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status,
    IReadOnlyCollection<Guid> RoleIds);
