namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Modules;

public sealed class ModuleDiscoveryTests
{
    [Fact(DisplayName = "Modules should have all expected layer assemblies when discovered")]
    public void Modules_Should_HaveAllExpectedLayerAssemblies_When_Discovered()
    {
        var discoveryErrors = ArchitectureDefinition.s_moduleDiscoveryErrors;
        var modules = ArchitectureDefinition.s_modules;

        Assert.Empty(discoveryErrors);
        Assert.NotEmpty(modules);
    }
}
