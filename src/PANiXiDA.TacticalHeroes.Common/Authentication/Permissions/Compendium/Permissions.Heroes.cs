using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Heroes
    {
        public const string ResourceCode = "heroes";
        public const string ResourceName = "Герои";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static Heroes()
        {
            (Create, Read, Update, Delete) = PermissionDefinition.CreateCrud(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                resourceGenitiveName: "героев");
        }
    }
}
