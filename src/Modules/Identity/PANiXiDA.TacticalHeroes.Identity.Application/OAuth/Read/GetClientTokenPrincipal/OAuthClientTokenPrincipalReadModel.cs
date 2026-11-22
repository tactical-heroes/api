using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetClientTokenPrincipal;

public sealed record OAuthClientTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
