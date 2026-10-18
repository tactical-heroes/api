namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

public sealed partial class RoleNames : Enumeration<RoleNames>
{
    public static readonly RoleNames Developer = new(id: 1, name: "developer");
    public static readonly RoleNames Moderator = new(id: 2, name: "moderator");
    public static readonly RoleNames Player = new(id: 3, name: "player");

    private RoleNames(int id, string name)
        : base(id, name)
    {
    }
}
