using System.Linq.Expressions;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

public sealed class UserByEmailSpecification : Specification<User>
{
    private readonly Email? _email;

    public UserByEmailSpecification(string email)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsSuccess)
        {
            _email = emailResult.Value;
        }
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        if (_email is null)
        {
            return user => false;
        }

        return user => user.Email == _email;
    }
}
