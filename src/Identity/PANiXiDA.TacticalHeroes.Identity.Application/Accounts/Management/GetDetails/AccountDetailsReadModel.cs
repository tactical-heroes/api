using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;

public sealed record AccountDetailsReadModel(
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
        AccountStatus.Blocked.Name,
        StringComparison.OrdinalIgnoreCase);
}
