using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Notifications.Presentation;

public static class PresentationAssembly
{
    public static Assembly Instance { get; } = typeof(PresentationAssembly).Assembly;
}
