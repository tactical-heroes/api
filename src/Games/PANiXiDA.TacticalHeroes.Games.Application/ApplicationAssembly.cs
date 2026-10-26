using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Games.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
