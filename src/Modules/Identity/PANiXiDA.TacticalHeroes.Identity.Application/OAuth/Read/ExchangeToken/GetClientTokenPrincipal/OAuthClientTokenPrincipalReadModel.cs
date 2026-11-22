using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetClientTokenPrincipal;

public sealed record OAuthClientTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
