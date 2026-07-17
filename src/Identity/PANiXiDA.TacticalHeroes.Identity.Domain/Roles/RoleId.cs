namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public readonly record struct RoleId(Guid Value)
{
    public static RoleId New()
    {
        return new RoleId(Value: Guid.CreateVersion7());
    }

    public static Result<RoleId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleId>(
                error: Error.Validation(message: "Role id cannot be empty."));
        }

        return Result.Success(value: new RoleId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
