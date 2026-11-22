using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.Create;

public sealed record CreateRoleRequest(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
