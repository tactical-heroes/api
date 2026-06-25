using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Organization.Product.ArchitectureTests.Domain;

public sealed class DomainArchitectureTests
{
    [Fact(DisplayName = "Domain value objects should be public")]
    public void DomainValueObjects_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.DomainAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("ValueObjects"))
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Domain policies should be public")]
    public void DomainPolicies_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.DomainAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Policies"))
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Domain specifications should be public")]
    public void DomainSpecifications_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.DomainAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Specifications"))
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }
}
