namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

public sealed partial class RoleName : Enumeration<RoleName>
{
    public static readonly RoleName Developer = new(id: 1, name: nameof(Developer), displayName: "Разработчик");
    public static readonly RoleName Moderator = new(id: 2, name: nameof(Moderator), displayName: "Модератор");
    public static readonly RoleName Player = new(id: 3, name: nameof(Player), displayName: "Игрок");

    private RoleName(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }
}
