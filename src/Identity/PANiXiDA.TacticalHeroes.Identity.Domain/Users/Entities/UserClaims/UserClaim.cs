using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

public sealed class UserClaim : Entity<UserClaimId>
{
    private UserClaim(
        UserClaimId id,
        ClaimType type,
        ClaimValue value)
        : base(id)
    {
        Type = type;
        Value = value;
    }

    public ClaimType Type { get; private set; }

    public ClaimValue Value { get; private set; }

    public static UserClaim Create(
        ClaimType type,
        ClaimValue value)
    {
        return new UserClaim(
            id: UserClaimId.New(),
            type: type,
            value: value);
    }
}
