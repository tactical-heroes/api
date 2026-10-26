using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
