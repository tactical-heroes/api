using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

public sealed record UpdateUserCommand(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status,
    IReadOnlyCollection<Guid> RoleIds,
    string? CurrentAccessToken = null) : ICommand<Result<bool>>;
