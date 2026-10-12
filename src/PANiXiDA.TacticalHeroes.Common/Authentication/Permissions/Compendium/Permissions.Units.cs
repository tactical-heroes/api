namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Compendium.Code);

        public static readonly PermissionDefinition Create =
            Resource.CreatePermission("create", "Создание юнитов");

        public static readonly PermissionDefinition Read =
            Resource.CreatePermission("read", "Просмотр юнитов");

        public static readonly PermissionDefinition Update =
            Resource.CreatePermission("update", "Изменение юнитов");

        public static readonly PermissionDefinition Delete =
            Resource.CreatePermission("delete", "Удаление юнитов");
    }
}
