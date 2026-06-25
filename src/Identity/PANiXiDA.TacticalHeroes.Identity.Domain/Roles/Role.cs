using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<RoleClaim> _claims = [];

    private Role(
        RoleId id,
        RoleName name)
        : base(id)
    {
        Name = name;
    }

    public RoleName Name { get; private set; }
    public IReadOnlyCollection<RoleClaim> Claims => _claims;

    public static Result<Role> Create(string name)
    {
        var nameResult = RoleName.Create(name);

        if (nameResult.IsFailure)
        {
            return Result.Failure<Role>(nameResult.Errors);
        }

        return Result.Success(new Role(RoleId.New(), nameResult.Value));
    }

    public Result GrantClaim(string type, string value)
    {
        var claimResult = RoleClaim.Create(type, value);

        if (claimResult.IsFailure)
        {
            return Result.Failure(claimResult.Errors);
        }

        if (_claims.Any(claim =>
                claim.Type == claimResult.Value.Type &&
                claim.Value == claimResult.Value.Value))
        {
            return Result.Success();
        }

        _claims.Add(claimResult.Value);

        return Result.Success();
    }

    public Result RevokeClaim(string type, string value)
    {
        var typeResult = ClaimType.Create(type);
        var valueResult = ClaimValue.Create(value);

        if (typeResult.IsFailure)
        {
            return Result.Failure(typeResult.Errors);
        }

        if (valueResult.IsFailure)
        {
            return Result.Failure(valueResult.Errors);
        }

        _claims.RemoveAll(claim =>
            claim.Type == typeResult.Value &&
            claim.Value == valueResult.Value);

        return Result.Success();
    }
}
