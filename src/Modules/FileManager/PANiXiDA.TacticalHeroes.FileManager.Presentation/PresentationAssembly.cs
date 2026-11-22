using System.Reflection;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
