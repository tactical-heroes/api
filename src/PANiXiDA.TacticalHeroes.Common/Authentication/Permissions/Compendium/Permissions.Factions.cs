using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static Factions()
        {
            var define = PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

            Create = define("create", "Создание фракций");
            Read = define("read", "Просмотр фракций");
            Update = define("update", "Изменение фракций");
            Delete = define("delete", "Удаление фракций");
        }
    }
}
