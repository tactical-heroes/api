using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Write.Create;

public sealed record CreateRoleCommand(
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims) : ICommand<Result<Guid>>;
