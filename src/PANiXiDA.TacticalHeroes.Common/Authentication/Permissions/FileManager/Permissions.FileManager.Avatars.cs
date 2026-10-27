namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class FileManager
    {
        public static class Avatars
        {
            public const string ResourceCode = "avatars";
            public const string ResourceName = "Аватары";

            public static readonly PermissionDefinition Read = PermissionDefinition.Create(
                moduleCode: Code,
                moduleDisplayName: DisplayName,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: "read",
                displayName: "Просмотр аватаров");
        }
    }
}
