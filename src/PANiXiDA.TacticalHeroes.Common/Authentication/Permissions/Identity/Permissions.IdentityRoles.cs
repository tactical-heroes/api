namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        private const string Prefix = $"{Sections.Identity.Code}.{ResourceCode}";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Identity.Code);

        public static readonly PermissionDefinition Create = new(
            Code: $"{Prefix}.create",
            DisplayName: "Создание ролей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Read = new(
            Code: $"{Prefix}.read",
            DisplayName: "Просмотр ролей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Update = new(
            Code: $"{Prefix}.update",
            DisplayName: "Изменение ролей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Delete = new(
            Code: $"{Prefix}.delete",
            DisplayName: "Удаление ролей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);
    }
}
