using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.GetDetails;

public sealed record GetRoleDetailsResponse(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
