using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    IReadOnlyCollection<Claim> Claims) : ICommand<Result>;
