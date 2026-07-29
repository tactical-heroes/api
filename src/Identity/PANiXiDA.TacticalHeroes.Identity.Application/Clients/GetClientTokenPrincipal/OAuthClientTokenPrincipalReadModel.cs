using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Clients.GetClientTokenPrincipal;

public sealed record OAuthClientTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims);
