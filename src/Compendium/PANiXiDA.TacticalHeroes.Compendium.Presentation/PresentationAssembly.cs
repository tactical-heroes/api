using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
