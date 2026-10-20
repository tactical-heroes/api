namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        public static readonly PermissionDefinition Create =
            DefineUnitPermission("create", "Создание юнитов");

        public static readonly PermissionDefinition Read =
            DefineUnitPermission("read", "Просмотр юнитов");

        public static readonly PermissionDefinition Update =
            DefineUnitPermission("update", "Изменение юнитов");

        public static readonly PermissionDefinition Delete =
            DefineUnitPermission("delete", "Удаление юнитов");

        private static PermissionDefinition DefineUnitPermission(string action, string displayName) =>
            PermissionDefinition.Create(
                moduleCode: Compendium.Code,
                moduleDisplayName: Compendium.DisplayName,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
