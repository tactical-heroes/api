namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetClientTokenPrincipal;

public sealed record GetClientTokenPrincipalQuery(string ClientId)
    : IQuery<Result<OAuthClientTokenPrincipalReadModel>>;
