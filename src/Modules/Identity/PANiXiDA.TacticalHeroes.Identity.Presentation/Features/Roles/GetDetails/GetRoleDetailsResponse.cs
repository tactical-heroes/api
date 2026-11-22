using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;

public sealed record GetRoleDetailsResponse(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims);
