namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;

public readonly record struct IdentityRoleId
{
    private IdentityRoleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static IdentityRoleId New()
    {
        return new IdentityRoleId(Guid.CreateVersion7());
    }

    public static Result<IdentityRoleId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<IdentityRoleId>(
                Error.Validation("Identity role id cannot be empty."));
        }

        return Result.Success(new IdentityRoleId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
