using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
