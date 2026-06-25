using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Organization.Product.ArchitectureTests.Presentation;

public sealed class PresentationArchitectureTests
{
    [Fact(DisplayName = "Presentation endpoints should be internal")]
    public void PresentationEndpoints_Should_Be_Internal()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.PresentationAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Features"))
            .And().HaveNameEndingWith("Endpoint")
            .Should().BeInternal()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Presentation request contracts should be internal")]
    public void PresentationRequestContracts_Should_Be_Internal()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.PresentationAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Features"))
            .And().HaveNameEndingWith("Request")
            .Should().BeInternal()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Presentation response contracts should be internal")]
    public void PresentationResponseContracts_Should_Be_Internal()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.PresentationAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Features"))
            .And().HaveNameEndingWith("Response")
            .Should().BeInternal()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Presentation mappers should be internal")]
    public void PresentationMappers_Should_Be_Internal()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.PresentationAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Features"))
            .And().HaveNameEndingWith("Mapper")
            .Should().BeInternal()
            .Check(ArchitectureDefinition.Architecture);
    }
}
