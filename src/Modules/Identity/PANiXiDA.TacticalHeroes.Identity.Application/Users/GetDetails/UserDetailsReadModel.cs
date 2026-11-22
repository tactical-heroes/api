using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetDetails;

public sealed record UserDetailsReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName,
    IReadOnlyCollection<Claim> Claims,
    IReadOnlyCollection<Guid> RoleIds) : IReadModel
{
    public bool IsBlocked => string.Equals(
        Status,
        UserStatus.Blocked.Name,
        StringComparison.OrdinalIgnoreCase);
}
