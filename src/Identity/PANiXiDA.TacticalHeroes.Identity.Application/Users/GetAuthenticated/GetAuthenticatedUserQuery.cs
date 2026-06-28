namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record GetAuthenticatedUserQuery(Guid UserId)
    : IQuery<Result<AuthenticatedUser>>;
