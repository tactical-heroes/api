namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

public interface IRolesRepository : IRepository<RoleId, Role>
{
    Task<IReadOnlyCollection<Role>> GetBySpecificationAsync(
        ISpecification<Role> specification,
        CancellationToken cancellationToken);
}
