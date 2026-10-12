namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        public static readonly ResourceDefinition Resource = new(ResourceCode, ResourceName, Sections.Compendium.Code);

        public static readonly PermissionDefinition Create =
            Resource.CreatePermission("create", "Создание героев");

        public static readonly PermissionDefinition Read =
            Resource.CreatePermission("read", "Просмотр героев");

        public static readonly PermissionDefinition Update =
            Resource.CreatePermission("update", "Изменение героев");

        public static readonly PermissionDefinition Delete =
            Resource.CreatePermission("delete", "Удаление героев");
    }
}
