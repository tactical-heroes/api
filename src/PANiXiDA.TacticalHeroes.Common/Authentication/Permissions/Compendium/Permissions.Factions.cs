namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Factions
    {
        public static readonly PermissionDefinition Create = new(
            Code: "compendium.factions.create",
            DisplayName: "Создание фракций",
            Group: "Справочники / Фракции");

        public static readonly PermissionDefinition Read = new(
            Code: "compendium.factions.read",
            DisplayName: "Просмотр фракций",
            Group: "Справочники / Фракции");

        public static readonly PermissionDefinition Update = new(
            Code: "compendium.factions.update",
            DisplayName: "Изменение фракций",
            Group: "Справочники / Фракции");

        public static readonly PermissionDefinition Delete = new(
            Code: "compendium.factions.delete",
            DisplayName: "Удаление фракций",
            Group: "Справочники / Фракции");
    }
}
