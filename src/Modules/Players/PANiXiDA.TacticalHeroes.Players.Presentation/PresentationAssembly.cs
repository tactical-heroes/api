using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Players.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
