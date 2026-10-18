namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

public sealed partial class RoleDefaultStatus : ValueObject
{
    private RoleDefaultStatus(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public bool IsDefault { get; }

    public static RoleDefaultStatus From(bool isDefault)
    {
        return new RoleDefaultStatus(isDefault: isDefault);
    }
}
