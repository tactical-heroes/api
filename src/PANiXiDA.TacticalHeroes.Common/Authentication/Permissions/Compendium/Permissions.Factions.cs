namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        private const string Prefix = $"{Sections.Compendium.Code}.{ResourceCode}";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Compendium.Code);

        public static readonly PermissionDefinition Create = new(
            Code: $"{Prefix}.create",
            DisplayName: "Создание фракций",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Read = new(
            Code: $"{Prefix}.read",
            DisplayName: "Просмотр фракций",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Update = new(
            Code: $"{Prefix}.update",
            DisplayName: "Изменение фракций",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Delete = new(
            Code: $"{Prefix}.delete",
            DisplayName: "Удаление фракций",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);
    }
}
