using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Login;

public sealed record AuthenticatedUserReadModel(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyCollection<Claim> Claims);
