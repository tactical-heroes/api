using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Write.Create;

public sealed record CreateRoleRequest(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
