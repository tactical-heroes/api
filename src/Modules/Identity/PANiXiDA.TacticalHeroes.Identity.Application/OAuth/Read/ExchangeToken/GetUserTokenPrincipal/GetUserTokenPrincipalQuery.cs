namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed record GetUserTokenPrincipalQuery(Guid UserId)
    : IQuery<Result<OAuthUserTokenPrincipalReadModel>>;
