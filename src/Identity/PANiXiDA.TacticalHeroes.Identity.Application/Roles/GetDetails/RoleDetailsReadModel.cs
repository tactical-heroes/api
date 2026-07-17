using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed record RoleDetailsReadModel(
    Guid Id,
    string Name,
    IReadOnlyCollection<Claim> Claims);
