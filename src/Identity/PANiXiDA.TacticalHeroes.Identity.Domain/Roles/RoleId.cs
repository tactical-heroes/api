namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public readonly record struct RoleId
{
    private RoleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RoleId New()
    {
        return new RoleId(Guid.CreateVersion7());
    }

    public static Result<RoleId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleId>(
                Error.Validation("Role id cannot be empty."));
        }

        return Result.Success(new RoleId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
