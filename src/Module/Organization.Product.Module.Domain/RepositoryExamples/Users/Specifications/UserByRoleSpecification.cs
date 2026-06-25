using System.Linq.Expressions;

namespace Organization.Product.Module.Domain.Users.Specifications;

public sealed class UserByRoleSpecification(string role) : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return item => item.Role.Name.Equals(role, StringComparison.CurrentCultureIgnoreCase);
    }
}
