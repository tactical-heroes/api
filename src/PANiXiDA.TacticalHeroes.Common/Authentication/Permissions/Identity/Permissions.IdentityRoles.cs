namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public static readonly PermissionDefinition Create = new(
            Code: "identity.roles.create",
            DisplayName: "Создание ролей",
            Group: "Роли");

        public static readonly PermissionDefinition Read = new(
            Code: "identity.roles.read",
            DisplayName: "Просмотр ролей",
            Group: "Роли");

        public static readonly PermissionDefinition Update = new(
            Code: "identity.roles.update",
            DisplayName: "Изменение ролей",
            Group: "Роли");

        public static readonly PermissionDefinition Delete = new(
            Code: "identity.roles.delete",
            DisplayName: "Удаление ролей",
            Group: "Роли");
    }
}
