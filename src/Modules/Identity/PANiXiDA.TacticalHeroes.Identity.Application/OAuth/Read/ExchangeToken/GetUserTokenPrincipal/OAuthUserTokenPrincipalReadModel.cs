using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed record OAuthUserTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
