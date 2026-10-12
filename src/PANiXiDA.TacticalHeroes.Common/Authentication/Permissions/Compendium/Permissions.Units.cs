namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        private const string Prefix = $"{Sections.Compendium.Code}.{ResourceCode}";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Compendium.Code);

        public static readonly PermissionDefinition Create = new(
            Code: $"{Prefix}.create",
            DisplayName: "Создание юнитов",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Read = new(
            Code: $"{Prefix}.read",
            DisplayName: "Просмотр юнитов",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Update = new(
            Code: $"{Prefix}.update",
            DisplayName: "Изменение юнитов",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Delete = new(
            Code: $"{Prefix}.delete",
            DisplayName: "Удаление юнитов",
            SectionCode: Sections.Compendium.Code,
            ResourceCode: ResourceCode);
    }
}
