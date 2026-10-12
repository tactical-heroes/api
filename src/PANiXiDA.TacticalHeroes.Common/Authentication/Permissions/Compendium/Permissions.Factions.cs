namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        public static readonly ResourceDefinition Resource = new(ResourceCode, ResourceName, Sections.Compendium.Code);

        public static readonly PermissionDefinition Create =
            Resource.CreatePermission("create", "Создание фракций");

        public static readonly PermissionDefinition Read =
            Resource.CreatePermission("read", "Просмотр фракций");

        public static readonly PermissionDefinition Update =
            Resource.CreatePermission("update", "Изменение фракций");

        public static readonly PermissionDefinition Delete =
            Resource.CreatePermission("delete", "Удаление фракций");
    }
}
