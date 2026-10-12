using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        public static readonly PermissionDefinition
            Create = Define("create", "Создание юнитов"),
            Read = Define("read", "Просмотр юнитов"),
            Update = Define("update", "Изменение юнитов"),
            Delete = Define("delete", "Удаление юнитов");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
