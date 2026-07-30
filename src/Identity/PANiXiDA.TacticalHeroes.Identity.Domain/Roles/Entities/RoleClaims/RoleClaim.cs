using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;

public sealed class RoleClaim : Entity<RoleClaimId>
{
    private RoleClaim(
        RoleClaimId id,
        ClaimType type,
        ClaimValue value)
        : base(id)
    {
        Type = type;
        Value = value;
    }

    public ClaimType Type { get; private set; }
    public ClaimValue Value { get; private set; }

    internal static Result<RoleClaim> Create(string type, string value)
    {
        var typeResult = ClaimType.Create(value: type);
        var valueResult = ClaimValue.Create(value: value);

        if (typeResult.IsFailure)
        {
            return Result.Failure<RoleClaim>(errors: typeResult.Errors);
        }

        if (valueResult.IsFailure)
        {
            return Result.Failure<RoleClaim>(errors: valueResult.Errors);
        }

        return Result.Success(value: new RoleClaim(
            id: RoleClaimId.New(),
            type: typeResult.Value,
            value: valueResult.Value));
    }
}
