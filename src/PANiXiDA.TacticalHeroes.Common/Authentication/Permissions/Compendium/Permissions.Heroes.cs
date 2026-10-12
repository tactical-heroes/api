namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Heroes
    {
        public static readonly PermissionDefinition Create = new(
            Code: "compendium.heroes.create",
            DisplayName: "Создание героев",
            Group: "Справочники / Герои");

        public static readonly PermissionDefinition Read = new(
            Code: "compendium.heroes.read",
            DisplayName: "Просмотр героев",
            Group: "Справочники / Герои");

        public static readonly PermissionDefinition Update = new(
            Code: "compendium.heroes.update",
            DisplayName: "Изменение героев",
            Group: "Справочники / Герои");

        public static readonly PermissionDefinition Delete = new(
            Code: "compendium.heroes.delete",
            DisplayName: "Удаление героев",
            Group: "Справочники / Герои");
    }
}
