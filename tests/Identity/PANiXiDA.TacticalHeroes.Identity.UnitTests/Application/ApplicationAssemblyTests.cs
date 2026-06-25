using PANiXiDA.TacticalHeroes.Identity.Application;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application;

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
