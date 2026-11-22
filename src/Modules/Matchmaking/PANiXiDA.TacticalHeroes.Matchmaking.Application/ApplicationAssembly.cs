using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
