using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        private static readonly Func<string, string, PermissionDefinition> s_define =
            PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

        public static readonly PermissionDefinition
            Create = s_define("create", "Создание фракций"),
            Read = s_define("read", "Просмотр фракций"),
            Update = s_define("update", "Изменение фракций"),
            Delete = s_define("delete", "Удаление фракций");
    }
}
