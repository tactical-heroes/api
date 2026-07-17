using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
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

    internal static Result<Role> Create(
        Guid id,
        string name,
        IEnumerable<(string Type, string Value)> claims)
    {
        var idResult = RoleId.Create(value: id);
        var nameResult = RoleName.Create(value: name);
        var validationResult = Result.Combine(idResult, nameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Role>(errors: validationResult.Errors);
        }

        var role = new Role(id: idResult.Value, name: nameResult.Value);

        foreach (var claim in claims)
        {
            var grantClaimResult = role.GrantClaim(claim.Type, claim.Value);

            if (grantClaimResult.IsFailure)
            {
                return Result.Failure<Role>(errors: grantClaimResult.Errors);
            }
        }

        return Result.Success(value: role);
    }

    public Result GrantClaim(string type, string value)
    {
        var claimResult = RoleClaim.Create(type: type, value: value);

        if (claimResult.IsFailure)
        {
            return Result.Failure(errors: claimResult.Errors);
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
        var typeResult = ClaimType.Create(value: type);
        var valueResult = ClaimValue.Create(value: value);

        if (typeResult.IsFailure)
        {
            return Result.Failure(errors: typeResult.Errors);
        }

        if (valueResult.IsFailure)
        {
            return Result.Failure(errors: valueResult.Errors);
        }

        _claims.RemoveAll(claim =>
            claim.Type == typeResult.Value &&
            claim.Value == valueResult.Value);

        return Result.Success();
    }
}
