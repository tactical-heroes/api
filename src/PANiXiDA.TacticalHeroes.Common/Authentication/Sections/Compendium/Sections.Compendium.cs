namespace PANiXiDA.TacticalHeroes.Common.Authentication.Sections;

public static partial class Sections
{
    public static class Compendium
    {
        public const string Code = "compendium";
        public const string DisplayName = "Справочники";

        public static readonly SectionDefinition Definition = new(Code, DisplayName);
    }
}
