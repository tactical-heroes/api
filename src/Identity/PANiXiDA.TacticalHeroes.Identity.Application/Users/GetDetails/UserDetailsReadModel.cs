using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

public sealed record UserDetailsReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName,
    IReadOnlyCollection<Claim> Claims)
{
    public bool IsBlocked => string.Equals(
        Status,
        UserStatus.Blocked.Name,
        StringComparison.OrdinalIgnoreCase);
}
