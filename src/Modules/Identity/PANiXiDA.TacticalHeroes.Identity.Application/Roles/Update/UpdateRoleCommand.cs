using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Update;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims) : ICommand<Result>;
