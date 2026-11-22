using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Games.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
