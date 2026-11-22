using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Write.Update;

public sealed record UpdateRoleRequest(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
