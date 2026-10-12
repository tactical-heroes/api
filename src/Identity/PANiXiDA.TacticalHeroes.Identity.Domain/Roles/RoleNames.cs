using System.Collections.Immutable;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public static class RoleNames
{
    public const string Developer = "developer";
    public const string Moderator = "moderator";
    public const string Player = "player";

    public static ImmutableArray<string> All { get; } = [Developer, Moderator, Player];
}
