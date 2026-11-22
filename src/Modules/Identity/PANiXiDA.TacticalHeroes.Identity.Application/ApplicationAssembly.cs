using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Identity.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
