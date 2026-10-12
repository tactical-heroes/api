using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Units
    {
        public const string ResourceCode = "units";
        public const string ResourceName = "Юниты";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static Units()
        {
            (Create, Read, Update, Delete) = PermissionDefinition.CreateCrud(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                resourceGenitiveName: "юнитов");
        }
    }
}
