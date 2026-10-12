using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        public static readonly PermissionDefinition
            Create = Define("create", "Создание героев"),
            Read = Define("read", "Просмотр героев"),
            Update = Define("update", "Изменение героев"),
            Delete = Define("delete", "Удаление героев");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
