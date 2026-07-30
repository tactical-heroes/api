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
    IReadOnlyCollection<Claim> Claims) : ReadModel
{
    public bool IsBlocked => string.Equals(
        a: Status,
        b: UserStatus.Blocked.Name,
        comparisonType: StringComparison.OrdinalIgnoreCase);
}
