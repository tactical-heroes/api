using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Create;

public sealed record CreateRoleCommand(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims) : ICommand<Result<Guid>>;
