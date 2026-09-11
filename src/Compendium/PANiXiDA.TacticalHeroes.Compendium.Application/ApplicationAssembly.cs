using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Compendium.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
