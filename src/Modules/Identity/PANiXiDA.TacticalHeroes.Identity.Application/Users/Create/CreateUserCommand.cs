using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Create;

public sealed record CreateUserCommand(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status,
    IReadOnlyCollection<Guid> RoleIds) : ICommand<Result<Guid>>;
