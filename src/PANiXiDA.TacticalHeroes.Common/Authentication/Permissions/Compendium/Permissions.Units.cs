namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Units
    {
        public static readonly PermissionDefinition Create = new(
            Code: "compendium.units.create",
            DisplayName: "Создание юнитов",
            Group: "Справочники / Юниты");

        public static readonly PermissionDefinition Read = new(
            Code: "compendium.units.read",
            DisplayName: "Просмотр юнитов",
            Group: "Справочники / Юниты");

        public static readonly PermissionDefinition Update = new(
            Code: "compendium.units.update",
            DisplayName: "Изменение юнитов",
            Group: "Справочники / Юниты");

        public static readonly PermissionDefinition Delete = new(
            Code: "compendium.units.delete",
            DisplayName: "Удаление юнитов",
            Group: "Справочники / Юниты");
    }
}
