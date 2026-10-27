namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class FileManager
    {
        public static class PersonalFiles
        {
            public const string ResourceCode = "personal-files";
            public const string ResourceName = "Персональные файлы";

            public static readonly PermissionDefinition Read = PermissionDefinition.Create(
                moduleCode: Code,
                moduleDisplayName: DisplayName,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: "read",
                displayName: "Просмотр персональных файлов");
        }
    }
}
