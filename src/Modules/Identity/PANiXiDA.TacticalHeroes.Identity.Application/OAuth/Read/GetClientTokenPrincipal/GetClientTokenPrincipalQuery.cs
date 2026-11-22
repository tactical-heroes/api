namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetClientTokenPrincipal;

public sealed record GetClientTokenPrincipalQuery(string ClientId)
    : IQuery<Result<OAuthClientTokenPrincipalReadModel>>;
