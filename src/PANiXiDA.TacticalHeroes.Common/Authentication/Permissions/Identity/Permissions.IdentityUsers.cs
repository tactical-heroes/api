namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public static readonly PermissionDefinition Create = new(
            Code: "identity.users.create",
            DisplayName: "Создание пользователей",
            Group: "Пользователи");

        public static readonly PermissionDefinition Read = new(
            Code: "identity.users.read",
            DisplayName: "Просмотр пользователей",
            Group: "Пользователи");

        public static readonly PermissionDefinition Update = new(
            Code: "identity.users.update",
            DisplayName: "Изменение пользователей",
            Group: "Пользователи");

        public static readonly PermissionDefinition Delete = new(
            Code: "identity.users.delete",
            DisplayName: "Удаление пользователей",
            Group: "Пользователи");
    }
}
