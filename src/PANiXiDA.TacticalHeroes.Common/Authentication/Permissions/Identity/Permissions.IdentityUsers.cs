namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public const string ResourceCode = "users";
        public const string ResourceName = "Пользователи";

        private const string Prefix = $"{Sections.Identity.Code}.{ResourceCode}";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Identity.Code);

        public static readonly PermissionDefinition Create = new(
            Code: $"{Prefix}.create",
            DisplayName: "Создание пользователей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Read = new(
            Code: $"{Prefix}.read",
            DisplayName: "Просмотр пользователей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Update = new(
            Code: $"{Prefix}.update",
            DisplayName: "Изменение пользователей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);

        public static readonly PermissionDefinition Delete = new(
            Code: $"{Prefix}.delete",
            DisplayName: "Удаление пользователей",
            SectionCode: Sections.Identity.Code,
            ResourceCode: ResourceCode);
    }
}
