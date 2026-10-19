using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed record RoleDetailsReadModel(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyCollection<Claim> Claims) : IReadModel;
