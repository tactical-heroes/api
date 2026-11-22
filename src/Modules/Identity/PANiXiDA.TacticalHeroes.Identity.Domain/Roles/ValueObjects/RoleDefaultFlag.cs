namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

public sealed partial class RoleDefaultFlag : ValueObject
{
    private RoleDefaultFlag(bool value)
    {
        Value = value;
    }

    public bool Value { get; }

    public static Result<RoleDefaultFlag> Create(bool value)
    {
        return Result.Success(value: new RoleDefaultFlag(value: value));
    }
}
