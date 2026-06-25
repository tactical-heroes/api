using System.Reflection;

namespace Organization.Product.Module.Application;

public static class ApplicationAssembly
{
    public static Assembly Instance { get; } = typeof(ApplicationAssembly).Assembly;
}
