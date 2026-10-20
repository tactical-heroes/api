namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Authorize;

public sealed record AuthorizeQuery(Guid UserId)
    : IQuery<Result<AuthorizeReadModel>>;
