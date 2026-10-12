using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        private static readonly Func<string, string, PermissionDefinition> s_define =
            PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

        public static readonly PermissionDefinition
            Create = s_define("create", "Создание юнитов"),
            Read = s_define("read", "Просмотр юнитов"),
            Update = s_define("update", "Изменение юнитов"),
            Delete = s_define("delete", "Удаление юнитов");
    }
}
