namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Authorize;

public sealed record AuthorizeQuery(Guid UserId)
    : IQuery<Result<AuthorizeReadModel>>;
