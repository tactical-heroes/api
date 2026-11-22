using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
