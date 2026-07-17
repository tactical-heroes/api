using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed record AuthenticatedAccountReadModel(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyCollection<Claim> Claims);
