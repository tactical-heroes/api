using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IAuthenticatedUserReader
{
    Task<Result<AuthenticatedUserReadModel>> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
