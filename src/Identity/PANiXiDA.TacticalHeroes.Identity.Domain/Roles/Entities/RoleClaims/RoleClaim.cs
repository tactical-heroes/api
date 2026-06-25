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
        var typeResult = ClaimType.Create(type);
        var valueResult = ClaimValue.Create(value);

        if (typeResult.IsFailure)
        {
            return Result.Failure<RoleClaim>(typeResult.Errors);
        }

        if (valueResult.IsFailure)
        {
            return Result.Failure<RoleClaim>(valueResult.Errors);
        }

        return Result.Success(new RoleClaim(
            RoleClaimId.New(),
            typeResult.Value,
            valueResult.Value));
    }
}
