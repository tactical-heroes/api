using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class Factions
    {
        public const string ResourceCode = "factions";
        public const string ResourceName = "Фракции";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static Factions()
        {
            (Create, Read, Update, Delete) = PermissionDefinition.CreateCrud(
                sectionCode: SectionCatalog.Compendium.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                resourceGenitiveName: "фракций");
        }
    }
}
