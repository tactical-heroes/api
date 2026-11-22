namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed record GetClientTokenPrincipalQuery(string ClientId)
    : IQuery<Result<OAuthClientTokenPrincipalReadModel>>;
