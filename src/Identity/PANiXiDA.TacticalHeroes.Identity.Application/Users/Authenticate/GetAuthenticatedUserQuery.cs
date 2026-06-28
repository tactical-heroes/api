namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authenticate;

public sealed record GetAuthenticatedUserQuery(Guid UserId)
    : IQuery<Result<AuthenticatedUserReadModel>>;
