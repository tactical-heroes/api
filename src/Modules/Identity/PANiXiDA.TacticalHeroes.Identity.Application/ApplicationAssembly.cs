using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
