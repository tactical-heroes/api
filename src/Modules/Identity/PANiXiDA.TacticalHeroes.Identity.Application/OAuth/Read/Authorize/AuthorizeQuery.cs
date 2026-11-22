namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.Authorize;

public sealed record AuthorizeQuery(Guid UserId)
    : IQuery<Result<AuthorizeReadModel>>;
