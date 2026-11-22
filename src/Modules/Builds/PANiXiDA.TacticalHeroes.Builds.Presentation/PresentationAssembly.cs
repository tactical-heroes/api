using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Builds.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
