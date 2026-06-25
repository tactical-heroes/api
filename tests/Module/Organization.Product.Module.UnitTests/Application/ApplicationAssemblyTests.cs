using Organization.Product.Module.Application;

namespace Organization.Product.Module.UnitTests.Application;

public sealed class ApplicationAssemblyTests
{
    [Fact(DisplayName = "Instance should return application assembly when accessed")]
    public void Instance_Should_Return_Application_Assembly_When_Accessed()
    {
        var expectedAssembly = typeof(ApplicationAssembly).Assembly;

        var assembly = ApplicationAssembly.Instance;

        assembly.ShouldBe(expectedAssembly);
    }
}
