using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        private static readonly Func<string, string, PermissionDefinition> s_define =
            PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

        public static readonly PermissionDefinition
            Create = s_define("create", "Создание героев"),
            Read = s_define("read", "Просмотр героев"),
            Update = s_define("update", "Изменение героев"),
            Delete = s_define("delete", "Удаление героев");
    }
}
