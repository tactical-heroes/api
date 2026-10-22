namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Modules;

public sealed class ModuleDependencyTests
{
    [Fact(DisplayName = "Module layers should not depend on other module internals when validated")]
    public void ModuleLayers_Should_NotDependOnOtherModuleInternals_When_Validated()
    {
        var modules = ArchitectureDefinition.s_modules;

        Assert.True(
            modules.Count > 1,
            "At least two modules are required to validate module isolation.");

        foreach (var sourceModule in modules)
        {
            foreach (var targetModule in modules.Where(module =>
                module != sourceModule))
            {
                foreach (var sourceAssemblyName in GetInternalAssemblyNames(
                    sourceModule))
                {
                    foreach (var targetAssemblyName in GetInternalAssemblyNames(
                        targetModule))
                    {
                        AssembliesShouldNotDependOn(
                            sourceAssemblyName,
                            targetAssemblyName);
                    }
                }
            }
        }
    }

    private static IReadOnlyCollection<string> GetInternalAssemblyNames(
        ModuleArchitecture module)
    {
        return
        [
            module.DomainAssemblyName,
            module.ApplicationAssemblyName,
            module.InfrastructureAssemblyName,
            module.PresentationAssemblyName
        ];
    }

    private static void AssembliesShouldNotDependOn(
        string sourceAssemblyName,
        string targetAssemblyName)
    {
        var sourceAssembly = ArchitectureDefinition.s_productionAssemblies
            .Single(assembly => assembly.GetName().Name == sourceAssemblyName);

        Assert.DoesNotContain(
            sourceAssembly.GetReferencedAssemblies(),
            reference => reference.Name == targetAssemblyName);
    }
}
