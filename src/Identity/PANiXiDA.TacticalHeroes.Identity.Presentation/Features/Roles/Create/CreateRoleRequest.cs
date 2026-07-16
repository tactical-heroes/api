using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

public sealed record CreateRoleRequest(
    string Name,
    IReadOnlyCollection<Claim> Claims);
