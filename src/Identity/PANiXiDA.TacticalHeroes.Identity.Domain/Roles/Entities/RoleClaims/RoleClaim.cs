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

    public static RoleClaim Create(ClaimType type, ClaimValue value)
    {
        return new RoleClaim(
            id: RoleClaimId.New(),
            type: type,
            value: value);
    }
}
