namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Modules;

public sealed class ModuleDependencyTests
{
    [Fact(DisplayName = "Module APIs should be implemented in owning Presentation when api contracts are declared")]
    public void ModuleApis_Should_BeImplementedInOwningPresentation_When_ApiContractsAreDeclared()
    {
        var implementations = ArchitectureDefinition.s_productionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .ToArray();
        var contracts = ArchitectureDefinition.s_modules
            .SelectMany(module => ArchitectureDefinition.s_productionAssemblies
                .Single(assembly => assembly.GetName().Name == module.ContractsAssemblyName)
                .GetTypes()
                .Where(type =>
                    type.IsInterface
                        && type.IsPublic
                        && type.Name.EndsWith("Api", StringComparison.Ordinal))
                .Select(contract => new { Module = module, Contract = contract }))
            .ToArray();

        var violations = contracts
            .Where(entry =>
            {
                var matches = implementations.Where(entry.Contract.IsAssignableFrom).ToArray();

                return matches.Length != 1
                    || matches[0].Assembly.GetName().Name != entry.Module.PresentationAssemblyName;
            })
            .Select(entry => $"{entry.Contract.FullName} must have exactly one implementation in "
                + $"{entry.Module.PresentationAssemblyName}.")
            .ToArray();

        Assert.NotEmpty(contracts);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

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
