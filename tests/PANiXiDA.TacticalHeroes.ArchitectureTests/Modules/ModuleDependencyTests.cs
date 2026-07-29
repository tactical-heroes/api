using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Modules;

public sealed class ModuleDependencyTests
{
    [Fact(DisplayName = "Module layers should not depend on other module internals when validated")]
    public void ModuleLayers_Should_NotDependOnOtherModuleInternals_When_Validated()
    {
        var modules = ArchitectureDefinition.Modules;

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
                        TypesShouldNotDependOn(
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

    private static void TypesShouldNotDependOn(
        string sourceAssemblyName,
        string targetAssemblyName)
    {
        Types()
            .That()
            .Are(ArchitectureDefinition.TypesInAssembly(sourceAssemblyName))
            .Should()
            .NotDependOnAny(
                ArchitectureDefinition.TypesInAssembly(targetAssemblyName))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureDefinition.Architecture);
    }
}
