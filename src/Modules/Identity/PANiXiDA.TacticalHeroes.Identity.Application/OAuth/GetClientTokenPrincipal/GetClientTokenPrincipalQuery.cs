namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed record GetClientTokenPrincipalQuery(string ClientId)
    : IQuery<Result<OAuthClientTokenPrincipalReadModel>>;
