using System.Linq.Expressions;

namespace Organization.Product.Module.Domain.Users.Specifications;

public sealed class UserBySearchQuerySpecification(string searchQuery) : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return item =>
            item.Name.Value.Contains(searchQuery) ||
            item.Email.Value.Contains(searchQuery);
    }
}
