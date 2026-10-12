namespace PANiXiDA.TacticalHeroes.Common.Authentication.Sections;

public static partial class Sections
{
    public static IReadOnlyList<SectionDefinition> All { get; } = Array.AsReadOnly(
    [
        Identity.Definition,
        Compendium.Definition
    ]);
}
