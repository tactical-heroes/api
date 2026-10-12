namespace PANiXiDA.TacticalHeroes.Common.Authentication.Sections;

public static partial class Sections
{
    public static class Identity
    {
        public const string Code = "identity";
        public const string DisplayName = "Управление доступом";

        public static readonly SectionDefinition Definition = new(Code, DisplayName);
    }
}
