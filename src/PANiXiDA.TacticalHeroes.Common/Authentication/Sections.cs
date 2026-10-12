namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static class Sections
{
    public static IReadOnlyList<SectionDefinition> All { get; } = Array.AsReadOnly<SectionDefinition>(
    [
        Identity.Definition,
        Compendium.Definition
    ]);

    public static class Identity
    {
        public const string Code = "identity";
        public const string DisplayName = "Управление доступом";

        public static readonly SectionDefinition Definition = new(Code, DisplayName);
    }

    public static class Compendium
    {
        public const string Code = "compendium";
        public const string DisplayName = "Справочники";

        public static readonly SectionDefinition Definition = new(Code, DisplayName);
    }
}
