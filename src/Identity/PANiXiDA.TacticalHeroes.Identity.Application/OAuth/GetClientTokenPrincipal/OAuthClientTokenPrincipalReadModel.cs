using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed record OAuthClientTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims);
