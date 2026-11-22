using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Abstractions;

public interface IAuthenticatedUserService
{
    Task<Result<AuthenticatedUserReadModel>> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
