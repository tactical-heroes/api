namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;

public readonly record struct RoleClaimId : IStronglyTypedId
{
    private RoleClaimId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RoleClaimId New()
    {
        return new RoleClaimId(value: Guid.CreateVersion7());
    }

    public static Result<RoleClaimId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleClaimId>(
                error: Error.Validation(message: "Role claim id cannot be empty."));
        }

        return Result.Success(value: new RoleClaimId(value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
