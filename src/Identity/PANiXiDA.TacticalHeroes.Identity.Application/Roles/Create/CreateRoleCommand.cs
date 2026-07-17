using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed record CreateRoleCommand(
    string Name,
    IReadOnlyCollection<Claim> Claims) : ICommand<Result<Guid>>;
