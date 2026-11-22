using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
