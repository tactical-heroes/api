namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

public sealed partial class RoleName : Enumeration<RoleName>
{
    public const int MaxNameLength = 128;

    public static readonly RoleName Developer = new(id: 1, name: nameof(Developer), displayName: "Разработчик");
    public static readonly RoleName Moderator = new(id: 2, name: nameof(Moderator), displayName: "Модератор");
    public static readonly RoleName Player = new(id: 3, name: nameof(Player), displayName: "Игрок");

    private RoleName(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public static RoleName GetDefault()
    {
        return Player;
    }

    public static Result<RoleName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RoleName>(
                error: Error.Validation(message: "Role name is required.")
                    .WithField(nameof(RoleName)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var roleName) && roleName is not null
            ? Result.Success(value: roleName)
            : Result.Failure<RoleName>(
                error: Error.Validation(message: $"Role name '{normalizedValue}' is invalid.")
                    .WithField(nameof(RoleName)));
    }
}
