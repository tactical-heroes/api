namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

public interface IRolesRepository : IRepository<RoleId, Role>
{
    Task<IReadOnlyCollection<Role>> GetByIdsAsync(
        IReadOnlyCollection<RoleId> roleIds,
        CancellationToken cancellationToken);
}
