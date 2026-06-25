namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;

public readonly record struct RoleClaimId
{
    private RoleClaimId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RoleClaimId New()
    {
        return new RoleClaimId(Guid.CreateVersion7());
    }

    public static Result<RoleClaimId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleClaimId>(
                Error.Validation("Role claim id cannot be empty."));
        }

        return Result.Success(new RoleClaimId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
