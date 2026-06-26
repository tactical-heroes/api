using System.Linq.Expressions;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Specifications;

public sealed class RolesByIdsSpecification : Specification<Role>
{
    private readonly RoleId[] _roleIds;

    public RolesByIdsSpecification(IEnumerable<Guid> roleIds)
    {
        ArgumentNullException.ThrowIfNull(roleIds);

        _roleIds = [.. roleIds
            .Select(RoleId.Create)
            .Where(roleIdResult => roleIdResult.IsSuccess)
            .Select(roleIdResult => roleIdResult.Value)
            .Distinct()];
    }

    public override Expression<Func<Role, bool>> ToExpression()
    {
        if (_roleIds.Length == 0)
        {
            return role => false;
        }

        return role => _roleIds.Contains(role.Id);
    }
}
