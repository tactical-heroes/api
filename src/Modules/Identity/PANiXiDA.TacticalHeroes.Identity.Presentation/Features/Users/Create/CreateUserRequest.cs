using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Create;

public sealed record CreateUserRequest(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status,
    IReadOnlyCollection<Guid> RoleIds);
