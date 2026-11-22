namespace PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;

public interface IBuildsApi
{
    Task<BuildResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
