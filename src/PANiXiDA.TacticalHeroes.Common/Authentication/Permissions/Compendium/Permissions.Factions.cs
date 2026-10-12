using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        public static readonly PermissionDefinition Create =
            Define("create", "Создание фракций");

        public static readonly PermissionDefinition Read =
            Define("read", "Просмотр фракций");

        public static readonly PermissionDefinition Update =
            Define("update", "Изменение фракций");

        public static readonly PermissionDefinition Delete =
            Define("delete", "Удаление фракций");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
