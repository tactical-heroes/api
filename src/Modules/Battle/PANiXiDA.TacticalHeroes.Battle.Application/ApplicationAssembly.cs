using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Battle.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
