using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        public static readonly PermissionDefinition Create =
            Define("create", "Создание героев");

        public static readonly PermissionDefinition Read =
            Define("read", "Просмотр героев");

        public static readonly PermissionDefinition Update =
            Define("update", "Изменение героев");

        public static readonly PermissionDefinition Delete =
            Define("delete", "Удаление героев");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
