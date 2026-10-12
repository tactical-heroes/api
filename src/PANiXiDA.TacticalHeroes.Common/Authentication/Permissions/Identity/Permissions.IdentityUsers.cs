using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public const string ResourceCode = "users";
        public const string ResourceName = "Пользователи";

        public static readonly PermissionDefinition
            Create = Define("create", "Создание пользователей"),
            Read = Define("read", "Просмотр пользователей"),
            Update = Define("update", "Изменение пользователей"),
            Delete = Define("delete", "Удаление пользователей");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
