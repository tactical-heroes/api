using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

public sealed record GetAuthenticatedUserQuery(
    Guid UserId,
    string SecurityStamp,
    string? AuthorizationId = null) : IQuery<Result<AuthenticatedUserReadModel>>;
