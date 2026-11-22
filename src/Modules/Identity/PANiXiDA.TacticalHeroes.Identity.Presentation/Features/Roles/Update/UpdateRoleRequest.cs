using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.Update;

public sealed record UpdateRoleRequest(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
