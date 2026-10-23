using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

public sealed record GetAuthenticatedUserQuery(Guid UserId) : IQuery<Result<AuthenticatedUserReadModel>>;
