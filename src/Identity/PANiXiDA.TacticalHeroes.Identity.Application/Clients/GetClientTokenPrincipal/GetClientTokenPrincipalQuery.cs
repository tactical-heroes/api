namespace PANiXiDA.TacticalHeroes.Identity.Application.Clients.GetClientTokenPrincipal;

public sealed record GetClientTokenPrincipalQuery(string ClientId)
    : IQuery<Result<OAuthClientTokenPrincipalReadModel>>;
