using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.GetAuthenticatedUser;

public sealed record GetAuthenticatedUserQuery(
    Guid UserId,
    string SecurityStamp,
    string? AuthorizationId = null) : IQuery<Result<AuthenticatedUserReadModel>>;
