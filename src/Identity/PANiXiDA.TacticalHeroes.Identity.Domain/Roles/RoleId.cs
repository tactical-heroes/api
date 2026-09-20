namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public readonly record struct RoleId : IStronglyTypedId
{
    private RoleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RoleId New()
    {
        return new RoleId(value: Guid.CreateVersion7());
    }

    public static Result<RoleId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleId>(
                error: Error.Validation(message: "Role id cannot be empty."));
        }

        return Result.Success(value: new RoleId(value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
