using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

public sealed record GetUserDetailsResponse(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName,
    IReadOnlyCollection<Claim> Claims);
