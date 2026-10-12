using ArchUnitNET.Domain;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Layers;

public sealed class LayerDependencyTests
{
    [Fact(DisplayName = "Domain layer should not depend on outer layers when validated")]
    public void DomainLayer_Should_NotDependOnOuterLayers_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_contractsLayer,
            ArchitectureDefinition.s_applicationLayer,
            ArchitectureDefinition.s_infrastructureLayer,
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer,
            ArchitectureDefinition.TypesInAssembly(ArchitectureDefinition.CommonAssemblyName)
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(
                ArchitectureDefinition.s_domainLayer,
                forbiddenDependency);
        }
    }

    [Fact(DisplayName = "Contracts layer should not depend on module layers or host when validated")]
    public void ContractsLayer_Should_NotDependOnModuleLayersOrHost_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_domainLayer,
            ArchitectureDefinition.s_applicationLayer,
            ArchitectureDefinition.s_infrastructureLayer,
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer,
            ArchitectureDefinition.TypesInAssembly(ArchitectureDefinition.CommonAssemblyName)
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(
                ArchitectureDefinition.s_contractsLayer,
                forbiddenDependency);
        }
    }

    [Fact(DisplayName = "Application layer should depend only on domain and shared abstractions when validated")]
    public void ApplicationLayer_Should_DependOnlyOnDomainAndSharedAbstractions_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_contractsLayer,
            ArchitectureDefinition.s_infrastructureLayer,
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(
                ArchitectureDefinition.s_applicationLayer,
                forbiddenDependency);
        }
    }

    [Fact(DisplayName = "Infrastructure layer should not depend on presentation or host when validated")]
    public void InfrastructureLayer_Should_NotDependOnPresentationOrHost_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(
                ArchitectureDefinition.s_infrastructureLayer,
                forbiddenDependency);
        }
    }

    [Fact(DisplayName = "Presentation layer should not depend on domain infrastructure or host when validated")]
    public void PresentationLayer_Should_NotDependOnDomainInfrastructureOrHost_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_contractsLayer,
            ArchitectureDefinition.s_domainLayer,
            ArchitectureDefinition.s_infrastructureLayer,
            ArchitectureDefinition.s_hostLayer
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(
                ArchitectureDefinition.s_presentationLayer,
                forbiddenDependency);
        }
    }

    [Fact(DisplayName = "Contracts layer should only be used by infrastructure when validated")]
    public void ContractsLayer_Should_OnlyBeUsedByInfrastructure_When_Validated()
    {
        var forbiddenSources = new[]
        {
            ArchitectureDefinition.s_domainLayer,
            ArchitectureDefinition.s_applicationLayer,
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer,
            ArchitectureDefinition.TypesInAssembly(ArchitectureDefinition.CommonAssemblyName)
        };

        foreach (var source in forbiddenSources)
        {
            TypesShouldNotDependOn(source, ArchitectureDefinition.s_contractsLayer);
        }
    }

    [Fact(DisplayName = "Common layer should not depend on module layers or host when validated")]
    public void CommonLayer_Should_NotDependOnModuleLayersOrHost_When_Validated()
    {
        var forbiddenDependencies = new[]
        {
            ArchitectureDefinition.s_contractsLayer,
            ArchitectureDefinition.s_domainLayer,
            ArchitectureDefinition.s_applicationLayer,
            ArchitectureDefinition.s_infrastructureLayer,
            ArchitectureDefinition.s_presentationLayer,
            ArchitectureDefinition.s_hostLayer
        };
        var commonLayer = ArchitectureDefinition.TypesInAssembly(ArchitectureDefinition.CommonAssemblyName);

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            TypesShouldNotDependOn(commonLayer, forbiddenDependency);
        }
    }

    private static void TypesShouldNotDependOn(
        IObjectProvider<IType> source,
        IObjectProvider<IType> forbiddenDependency)
    {
        Types().That()
            .Are(source)
            .Should()
            .NotDependOnAny(forbiddenDependency)
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureDefinition.s_architecture);
    }
}
