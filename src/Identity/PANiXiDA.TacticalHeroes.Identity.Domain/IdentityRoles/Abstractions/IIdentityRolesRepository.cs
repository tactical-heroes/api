namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Abstractions;

public interface IIdentityRolesRepository : IRepository<IdentityRoleId, IdentityRole>
{
    Task<IReadOnlyCollection<IdentityRole>> GetByIdsAsync(
        IReadOnlyCollection<IdentityRoleId> roleIds,
        CancellationToken cancellationToken);
}
