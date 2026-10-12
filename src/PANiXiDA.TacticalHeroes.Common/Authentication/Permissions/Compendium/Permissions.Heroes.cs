namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        private const string Prefix = $"{Sections.Compendium.Code}.{ResourceCode}";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Compendium.Code);

        public static readonly PermissionDefinition Create = new(
            Code: $"{Prefix}.create",
            DisplayName: "Создание героев",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Read = new(
            Code: $"{Prefix}.read",
            DisplayName: "Просмотр героев",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Update = new(
            Code: $"{Prefix}.update",
            DisplayName: "Изменение героев",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Delete = new(
            Code: $"{Prefix}.delete",
            DisplayName: "Удаление героев",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);
    }
}
